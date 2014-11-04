﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using GSoft.Dynamite.Globalization.Variations;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.TimerJobs;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Helpers
{
    /// <summary>
    /// SharePoint variations helpers
    /// </summary>
    public class VariationHelper : IVariationHelper
    {
        private readonly ILogger logger;
        private readonly ITimerJobHelper timerJobHelper;

        /// <summary>
        /// Default constructor with dependency injection
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="timerJobHelper">The timer job helper</param>
        public VariationHelper(ILogger logger, ITimerJobHelper timerJobHelper)
        {
            this.logger = logger;
            this.timerJobHelper = timerJobHelper;
        }

        /// <summary>
        /// Determines whether [the specified web] [is current web source label].
        /// </summary>
        /// <param name="web">The web.</param>
        /// <returns>A boolean value which indicates if the current web is the source variation label.</returns>
        public bool IsCurrentWebSourceLabel(SPWeb web)
        {
            var sourceLabel = Microsoft.SharePoint.Publishing.Variations.GetLabels(web.Site).FirstOrDefault(x => x.IsSource);
            if (sourceLabel != null)
            {
                // Compare absolute URL values
                return web.Url.StartsWith(sourceLabel.TopWebUrl, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Determines if variations are enabled on a site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>A boolean value which indicates if the current site has variations enabled.</returns>
        public bool IsVariationsEnabled(SPSite site)
        {
            bool isEnabled = Microsoft.SharePoint.Publishing.Variations.GetLabels(site).Count > 0;

            return isEnabled;
        }

        /// <summary>
        /// Get the variations labels for the site collection.
        /// NOTE: Also possible with the static Microsoft.SharePoint.Publishing Variations object by faking a SPContext
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="labelToSync">The label name to Sync. example: <c>"en"</c> or <c>"fr"</c>.</param>
        /// <returns>A collection of unique label.</returns>
        public ReadOnlyCollection<Microsoft.SharePoint.Publishing.VariationLabel> GetVariationLabels(SPSite site, string labelToSync)
        {
            this.logger.Info("Start method 'GetVariationLabels' for site url: '{0}' with label '{1}'", site.Url, labelToSync);

            var web = site.RootWeb;
            var variationLabelsList = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, "/Variation Labels/Allitems.aspx"));
            var list = new List<Microsoft.SharePoint.Publishing.VariationLabel>();
            var query = new SPQuery
            {
                Query = "<Where><Eq><FieldRef Name='Title'/><Value Type='Text'>" + labelToSync + "</Value></Eq></Where><OrderBy><FieldRef Name=\"Title\" Ascending=\"TRUE\"></FieldRef></OrderBy>"
            };

            foreach (SPListItem item in variationLabelsList.GetItems(query))
            {
                var webUrl = (string)item["Top Web URL"];
                webUrl = webUrl.Substring(webUrl.IndexOf(',') + 1);

                var pubWeb = PublishingWeb.GetPublishingWeb(site.OpenWeb(webUrl));
                var varLbl = pubWeb.Label;
                list.Add(varLbl);
            }

            return new ReadOnlyCollection<Microsoft.SharePoint.Publishing.VariationLabel>(list);
        }

        /// <summary>
        /// Get the variations labels for the site collection.
        /// NOTE: Also possible with the static Microsoft.SharePoint.Publishing Variations object by faking a SPContext
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>A collection of unique label.</returns>
        public ReadOnlyCollection<Microsoft.SharePoint.Publishing.VariationLabel> GetVariationLabels(SPSite site)
        {
            this.logger.Info("Start method 'GetVariationLabels' for site url: '{0}'", site.Url);

            var web = site.RootWeb;
            var variationLabelsList = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, "/Variation Labels/Allitems.aspx"));
            var list = new List<Microsoft.SharePoint.Publishing.VariationLabel>();
            var query = new SPQuery
            {
                Query = "<OrderBy><FieldRef Name=\"Title\" Ascending=\"TRUE\"></FieldRef></OrderBy>"
            };

            foreach (SPListItem item in variationLabelsList.GetItems(query))
            {
                var webUrl = (string)item["Top Web URL"];
                webUrl = webUrl.Substring(webUrl.IndexOf(',') + 1);

                var pubWeb = PublishingWeb.GetPublishingWeb(site.OpenWeb(webUrl));
                var varLbl = pubWeb.Label;
                list.Add(varLbl);
            }

            return new ReadOnlyCollection<Microsoft.SharePoint.Publishing.VariationLabel>(list);
        }

        /// <summary>
        /// The configure variations settings method.
        /// </summary>
        /// <param name="site">
        /// The site collection.
        /// </param>
        /// <param name="variationSettings">The variations settings</param>
        public void EnsureVariationsSettings(SPSite site, VariationSettingsInfo variationSettings)
        {
            var rootWeb = site.RootWeb;
            Guid varRelationshipsListId = new Guid(rootWeb.AllProperties["_VarRelationshipsListId"] as string);
            SPList varRelationshipsList = rootWeb.Lists[varRelationshipsListId];
            SPFolder rootFolder = varRelationshipsList.RootFolder;

            // Automatic creation
            rootFolder.Properties["EnableAutoSpawnPropertyName"] = variationSettings.EnableAutoSpawn;

            // Recreate Deleted Target Page; set to false to enable recreation
            rootFolder.Properties["AutoSpawnStopAfterDeletePropertyName"] = variationSettings.AutoSpawnStopAfterDelete;

            // Update Target Page Web Parts
            rootFolder.Properties["UpdateWebPartsPropertyName"] = variationSettings.UpdateWebParts;

            // Resources
            rootFolder.Properties["CopyResourcesPropertyName"] = variationSettings.CopyResources;

            // Notification
            rootFolder.Properties["SendNotificationEmailPropertyName"] = variationSettings.SendNotificationEmail;
            rootFolder.Properties["SourceVarRootWebTemplatePropertyName"] = variationSettings.SourceVarRootWebTemplate;
            rootFolder.Update();

            SPListItem item = null;
            if (varRelationshipsList.Items.Count > 0)
            {
                item = varRelationshipsList.Items[0];
            }
            else
            {
                item = varRelationshipsList.Items.Add();
                item["GroupGuid"] = new Guid("F68A02C8-2DCC-4894-B67D-BBAED5A066F9");
            }

            item["Deleted"] = false;
            item["ObjectID"] = rootWeb.ServerRelativeUrl;
            item["ParentAreaID"] = string.Empty;

            item.Update();
        }

        /// <summary>
        /// The create variations method.
        /// </summary>
        /// <param name="site">
        /// The site collection.
        /// </param>
        /// <param name="labels">The label metadata for all that should be synched. example: <c>"en"</c> or <c>"fr"</c>.</param>
        public void EnsureVariationlabels(SPSite site, IList<VariationLabelInfo> labels)
        {
            var rootWeb = site.RootWeb;
            Guid varListId = new Guid(rootWeb.AllProperties["_VarLabelsListId"] as string);
            SPList varList = rootWeb.Lists[varListId];

            foreach (VariationLabelInfo label in labels)
            {
                SPListItem item;
                var existingItems = varList.Items.Cast<SPListItem>().Where(listItem => listItem.Title == label.Title).ToList();

                if (existingItems.Count > 0)
                {
                    item = existingItems[0];
                }
                else
                {
                    // create the label
                    item = varList.Items.Add();
                }

                item[SPBuiltInFieldId.Title] = label.Title;
                item["Description"] = label.Description;
                item["Flag Control Display Name"] = label.FlagControlDisplayName;
                item["Language"] = label.Language;
                item["Locale"] = label.Locale.ToString(CultureInfo.InvariantCulture);
                item["Hierarchy Creation Mode"] = label.HierarchyCreationMode;
                item["Is Source"] = label.IsSource.ToString();

                if (existingItems.Count > 0)
                {
                    // assume hierarchy already exists also
                    item["Hierarchy Is Created"] = true;
                }
                else
                {
                    item["Hierarchy Is Created"] = false;
                }

                item.Update();
            }
        }

        /// <summary>
        /// The create hierarchies.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="labels">The variation labels</param>
        public void CreateHierarchies(SPSite site, IList<VariationLabelInfo> labels)
        {
            this.timerJobHelper.CreateJob(site, new Guid("e7496be8-22a8-45bf-843a-d1bd83aceb25"));

            var jobId = this.timerJobHelper.StartJob(site, "VariationsCreateHierarchies");

            DateTime startTime = DateTime.Now.ToUniversalTime();

            this.timerJobHelper.WaitForJob(site, jobId, startTime);

            // Force the title of the label subsites, because the value of Flag Control Display Name doesn't get respected on destination labels most of the time.
            // Also take care of setting the regional settings on each site.
            foreach (VariationLabelInfo label in labels)
            {
                using (var labelWeb = site.OpenWeb(label.Title))
                {
                    // UICulture's gotta be in the same locale as the web being renamed (otherwise change won't go through - thanks MUI!)
                    var previousUiCulture = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(label.Language);

                    labelWeb.Title = label.FlagControlDisplayName;
                    labelWeb.Update();

                    Thread.CurrentThread.CurrentUICulture = previousUiCulture;
                }
            }
        }

        /// <summary>
        /// Setup variations on a site
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="variationSettings">The variation settings</param>
        public void SetupVariations(SPSite site, VariationSettingsInfo variationSettings)
        {
            // Configure varaitions settings
            this.EnsureVariationsSettings(site, variationSettings);
            
            // Create labels
            this.EnsureVariationlabels(site, variationSettings.Labels);

            // Create hierachies
            this.CreateHierarchies(site, variationSettings.Labels);
        }

        /// <summary>
        /// Get the hidden relationships list for a site collection.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The relationships list.</returns>
        private SPList GetVariationLabelHiddenList(SPSite site)
        {
            var guid = new Guid(site.RootWeb.GetProperty("_VarLabelsListId").ToString());
            return site.RootWeb.Lists[guid];
        }

        /// <summary>
        /// Get the hidden variation labels for a site collection.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>the variation labels list.</returns>
        private SPList GetRelationshipsHiddenList(SPSite site)
        {
            var guid = new Guid(site.RootWeb.GetProperty("_VarRelationshipsListId").ToString());
            return site.RootWeb.Lists[guid];
        }
    }
}
