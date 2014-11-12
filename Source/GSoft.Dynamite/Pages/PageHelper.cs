﻿using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using GSoft.Dynamite.Events;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Pages
{
    /// <summary>
    /// Helper class for SharePoint publishing pages
    /// </summary>
    public class PageHelper : IPageHelper
    {
        private readonly IWebPartHelper webPartHelper;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new <see cref="PageHelper" /> instance
        /// </summary>
        /// <param name="webPartHelper">Web Part helper</param>
        /// <param name="logger">The logger.</param>
        public PageHelper(IWebPartHelper webPartHelper, ILogger logger)
        {
            this.webPartHelper = webPartHelper;
            this.logger = logger;
        }

        /// <summary>
        /// Ensure a collection of pages in a folder
        /// </summary>
        /// <param name="library">The library</param>
        /// <param name="folder">The folder</param>
        /// <param name="pages">The page information</param>
        /// <returns>A collection of publishing pages</returns>
        public IEnumerable<PublishingPage> EnsurePage(SPList library, SPFolder folder, ICollection<PageInfo> pages)
        {
            return pages.Select(page => this.EnsurePage(library, folder, page)).ToList();
        }

        /// <summary>
        /// Ensure a publishing page in a folder
        /// </summary>
        /// <param name="library">The library</param>
        /// <param name="folder">The folder</param>
        /// <param name="page">The page information</param>
        /// <returns>The publishing page object</returns>
        public PublishingPage EnsurePage(SPList library, SPFolder folder, PageInfo page)
        {
            var publishingSite = new PublishingSite(library.ParentWeb.Site);
            var publishingWeb = PublishingWeb.GetPublishingWeb(library.ParentWeb);
            var publishingPages = publishingWeb.GetPublishingPages();

            PageLayout pageLayout;

            // Get the correct Page Layout according to its name (<xxx>.aspx)
            var pageLayoutInfo = this.GetPageLayout(publishingSite, page.PageLayout.Name, true);

            // If a page layout was specified and its from the correct web.
            if (pageLayoutInfo != null && pageLayoutInfo.ListItem.ParentList.ParentWeb.ID == publishingSite.RootWeb.ID)
            {
                // Associate the page layout specified in the page.
                pageLayout = pageLayoutInfo;
            }
            else
            {
                // Get the first page layout with the specified content type id.
                var pageContentType = publishingSite.ContentTypes[page.ContentTypeId];
                var pageLayouts = publishingSite.GetPageLayouts(pageContentType, true);
                pageLayout = pageLayouts[0]; // default to first associated page layout
            }

            var pageServerRelativeUrl = folder.ServerRelativeUrl + "/" + page.FileName + ".aspx";
            var publishingPage = publishingWeb.GetPublishingPage(pageServerRelativeUrl);

            if (publishingPage == null)
            {
                // Only create the page if it doesn't exist yet and allow event firing on ItemAdded
                publishingPage = publishingPages.Add(pageServerRelativeUrl, pageLayout);
            }
            else
            {
                this.EnsurePageCheckOut(publishingPage);
            }

            // Set the title
            if (!string.IsNullOrEmpty(page.Title))
            {
                publishingPage.Title = page.Title;
                publishingPage.Update();
            }
            
            // Insert WebParts
            foreach (WebPartInfo webPartSetting in page.WebParts)
            {
                this.webPartHelper.EnsureWebPartToZone(publishingPage.ListItem, webPartSetting.WebPart, webPartSetting.ZoneName, webPartSetting.ZoneIndex);
            }

            // Publish
            PageHelper.EnsurePageCheckInAndPublish(publishingPage);
            
            return publishingPage;
        }

        /// <summary>
        /// Get the page layout
        /// </summary>
        /// <param name="publishingSite">the current publishing site</param>
        /// <param name="pageLayoutName">the page layout name</param>
        /// <param name="excludeObsolete">exclude obsolete page layout</param>
        /// <returns>the page layout</returns>
        public PageLayout GetPageLayout(PublishingSite publishingSite, string pageLayoutName, bool excludeObsolete)
        {
            return publishingSite.GetPageLayouts(excludeObsolete).Cast<PageLayout>().FirstOrDefault(pageLayout => pageLayout.Name == pageLayoutName);
        }

        /// <summary>
        /// Configures a page layout
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="pageLayoutInfo">The page layout info</param>
        /// <returns>The page layout</returns>
        public PageLayout EnsurePageLayout(SPSite site, PageLayoutInfo pageLayoutInfo)
        {
            var publishingSite = new PublishingSite(site);
            var pageLayout = this.GetPageLayout(publishingSite, pageLayoutInfo.Name, true);

            if (!string.IsNullOrEmpty(pageLayoutInfo.AssociatedContentTypeId))
            {
                var contentTypeId =
                site.RootWeb.ContentTypes.BestMatch(new SPContentTypeId(pageLayoutInfo.AssociatedContentTypeId));

                var ct = site.RootWeb.ContentTypes[contentTypeId];

                // Update the publishing associated content type
                pageLayout.AssociatedContentType = ct;
                pageLayout.Update();
            }

            return pageLayout;
        }

        private void EnsurePageCheckOut(PublishingPage page)
        {
            if (page.ListItem.ParentList.ForceCheckout)
            {
                // Only check out if we are forced to do so
                if (page.ListItem.File.CheckOutType == SPFile.SPCheckOutType.None)
                {
                    // Only check out if not already checked out
                    page.CheckOut();
                }
                else
                {
                    this.logger.Warn("Page " + page.Uri.AbsoluteUri + " is already checked out - skipping FolderMaker checkout.");
                }
            }
        }

        private static void EnsurePageCheckInAndPublish(PublishingPage page)
        {
            string comment = "Dynamite Ensure Creation";

            if (page.ListItem.File.CheckOutType != SPFile.SPCheckOutType.None)
            {
                // Only check in if already checked out
                page.CheckIn(comment);
            }

            if (page.ListItem.ModerationInformation.Status == SPModerationStatusType.Draft)
            {
                // Create a major version (just like "submit for approval")
                page.ListItem.File.Publish(comment);

                // Status should now be Pending. Approve to make the major version visible to the public.
                page.ListItem.File.Approve(comment);
            }
            else if (page.ListItem.ModerationInformation.Status == SPModerationStatusType.Pending)
            {
                // Technically, major version already exists, we just need to approve in order for the major version to be published
                page.ListItem.File.Approve(comment);
            }
        }
    }
}
