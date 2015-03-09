﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Lists.Constants;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Pages;
using GSoft.Dynamite.ValueTypes.Writers;
using Microsoft.Office.DocumentManagement;
using Microsoft.Office.Server.Search.Internal.UI;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Portal.WebControls;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Folders
{
    /// <summary>
    /// Helper class for SharePoint folders
    /// </summary>
    public class FolderHelper : IFolderHelper
    {
        private readonly ILogger logger;
        private readonly IPageHelper pageHelper;
        private readonly IFieldValueWriter valueWriter;

        /// <summary>
        /// Constructor for FolderHelper
        /// </summary>
        /// <param name="logger">The logger helper instance</param>
        /// <param name="pageHelper">The page helper instance</param>
        /// <param name="valueWriter">Field value initializer</param>
        public FolderHelper(ILogger logger, IPageHelper pageHelper, IFieldValueWriter valueWriter)
        {
            this.logger = logger;
            this.pageHelper = pageHelper;
            this.valueWriter = valueWriter;
        }

        /// <summary>
        /// Ensure a folder hierarchy in a list or library
        /// </summary>
        /// <param name="list">The SharePoint list or library</param>
        /// <param name="rootFolderInfo">The metadata that should define the root folder of the list</param>
        /// <returns>The list's root folder instance</returns>
        public SPFolder EnsureFolderHierarchy(SPList list, FolderInfo rootFolderInfo)
        {
            return this.EnsureFolder(list, null, rootFolderInfo);
        }

        /// <summary>
        /// Method to revert to home page to the default web page
        /// </summary>
        /// <param name="web">The web</param>
        public void ResetWelcomePageToDefault(SPWeb web)
        {
            web.RootFolder.WelcomePage = "Pages/default.aspx";
            web.RootFolder.Update();
        }

        private SPFolder EnsureFolder(SPList library, SPFolder parentFolder, FolderInfo folderInfo)
        {
            SPFolder folder = null;

            if (parentFolder == null)
            {
                // We are on at the root folder of the library (i.e. no parent folder).
                // Initialize defaults and pages in here, then move on to subfolders (instead of trying to create the folder)
                folder = library.RootFolder;

                // Ensure folder metadata defaults
                bool isDocumentLibrary = library.BaseType == SPBaseType.DocumentLibrary;
                if (folderInfo.FieldDefaultValues != null && folderInfo.FieldDefaultValues.Count > 0)
                {
                    if (!isDocumentLibrary)
                    {
                        throw new ArgumentException("EnsureFolderHierarchy - Impossible to ensure folder MetadataDefaults on a list which is not a Document Library.");
                    }

                    this.valueWriter.WriteValuesToFolderDefaults(folder, folderInfo.FieldDefaultValues.ToList());
                }
                else if (isDocumentLibrary)
                {
                    ClearFolderAllFolderMetadataDefaults(folder);
                }

                // UniqueContentTypeOrder
                EnsureUniqueContentTypeOrder(folder, folderInfo);

                // Create pages
                if (folderInfo.Pages != null && folderInfo.Pages.Count > 0)
                {
                    if ((int)library.BaseTemplate != BuiltInListTemplates.Pages.ListTempateTypeId)
                    {
                        // To provision Publishing Pages, you NEED to be inside the Pages library
                        throw new ArgumentException("Publishing pages cannot be provisionned outside of the Pages library. Remove the PageInfo objects from your FolderInfo, or use this FolderInfo to provision content inside the Pages library instead.");
                    }

                    this.pageHelper.EnsurePage(library, folder, folderInfo.Pages);
                }

                // Create sub folders
                if (folderInfo.Subfolders != null && folderInfo.Subfolders.Count > 0)
                {
                    library.EnableFolderCreation = true;
                    library.Update();

                    foreach (var childFolder in folderInfo.Subfolders)
                    {
                        this.EnsureFolder(library, folder, childFolder);
                    }
                }

                // Set Web HomePage
                if (folderInfo.WelcomePage != null)
                {
                    var rootFolder = library.ParentWeb.RootFolder;
                    rootFolder.WelcomePage = folderInfo.WelcomePage.LibraryRelativePageUrl.ToString();
                    rootFolder.Update();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(folderInfo.Name))
                {
                    bool isDocumentLibrary = library.BaseType == SPBaseType.DocumentLibrary;

                    try
                    {
                        if (isDocumentLibrary)
                        {
                            folder = parentFolder.SubFolders[folderInfo.Name];
                        }
                        else
                        {
                            string serverRelativeFolderPathToMatch = SPUtility.ConcatUrls(parentFolder.ServerRelativeUrl, folderInfo.Name);
                            folder = library.Folders.Cast<SPListItem>().Single(folderItem => folderItem.Folder.ServerRelativeUrl == serverRelativeFolderPathToMatch).Folder;
                        }

                        this.logger.Info("Skipping folder creation for " + folderInfo.Name +
                                         " because it already exists (will still apply values and default metadata)");
                    }
                    catch (ArgumentException)
                    {
                        this.logger.Info("Creating liubrary folder " + folderInfo.Name);
                    }
                    catch (InvalidOperationException)
                    {
                        this.logger.Info("Creating list folder " + folderInfo.Name);
                    }

                    if (folder == null)
                    {
                        // Add the folder (if it doesn't already exist)
                        if (isDocumentLibrary)
                        {
                            folder = parentFolder.SubFolders.Add(folderInfo.Name);
                        }
                        else
                        {
                            // In a non-doclib list, creating Folders needs to be done in another way (otherwise the SPFolder will not get an associated SPListItem)
                            var folderItem = library.Items.Add(parentFolder.ServerRelativeUrl, SPFileSystemObjectType.Folder);
                            folderItem["Title"] = folderInfo.Name;
                            folderItem.Update();

                            folder = folderItem.Folder;
                        }
                    }

                    // Ensure folder metadata defaults
                    if (folderInfo.FieldDefaultValues != null && folderInfo.FieldDefaultValues.Count > 0)
                    {
                        if (!isDocumentLibrary)
                        {
                            throw new ArgumentException("EnsureFolderHierarchy - Impossible to ensure folder MetadataDefaults on a list which is not a Document Library.");
                        }

                        this.valueWriter.WriteValuesToFolderDefaults(folder, folderInfo.FieldDefaultValues.ToList());
                    }
                    else if (isDocumentLibrary)
                    {
                        ClearFolderAllFolderMetadataDefaults(folder);
                    }

                    // UniqueContentTypeInfo
                    EnsureUniqueContentTypeOrder(folder, folderInfo);

                    // Make sure the folder is published
                    SPModerationInformation folderModerationInfo = folder.Item != null ? folder.Item.ModerationInformation : null;
                    if (folderModerationInfo != null)
                    {
                        folderModerationInfo.Comment = "Automatically approved upon creation through Dynamite's FolderHelper utility.";
                        folderModerationInfo.Status = SPModerationStatusType.Approved;
                        folder.Item.Update();
                    }

                    // Create pages
                    if (folderInfo.Pages != null && folderInfo.Pages.Count > 0)
                    {
                        if ((int)library.BaseTemplate != BuiltInListTemplates.Pages.ListTempateTypeId)
                        {
                            // To provision Publishing Pages, you NEED to be inside the Pages library
                            throw new ArgumentException("Publishing pages cannot be provisionned outside of the Pages library. Remove the PageInfo objects from your FolderInfo, or use this FolderInfo to provision content inside the Pages library instead.");
                        }
                    }

                    this.pageHelper.EnsurePage(library, folder, folderInfo.Pages);
                }

                // Create sub folders
                foreach (var childFolder in folderInfo.Subfolders)
                {
                    this.EnsureFolder(library, folder, childFolder);
                }
            }

            return folder;
        }

        private static void EnsureUniqueContentTypeOrder(SPFolder folder, FolderInfo folderInfo)
        {
            if (folderInfo.UniqueContentTypeOrder != null && folderInfo.UniqueContentTypeOrder.Count > 0)
            {
                SPContentTypeCollection listContentTypes = folder.ParentWeb.Lists[folder.ParentListId].ContentTypes;
                IList<SPContentType> uniqueOrder = new List<SPContentType>();

                foreach (ContentTypeInfo needToEnsureThisCTInfo in folderInfo.UniqueContentTypeOrder)
                {
                    var foundRealCT = listContentTypes[needToEnsureThisCTInfo.ContentTypeId];

                    if (foundRealCT == null)
                    {
                        // Failed to find CT with direct ID lookup: now let's assume that we were given site collection CT IDs as
                        // input and that we need to inspect the list CTs' parents to find a match.
                        foreach (var listCT in listContentTypes.Cast<SPContentType>())
                        {
                            if (listCT.Id.Parent == needToEnsureThisCTInfo.ContentTypeId)
                            {
                                foundRealCT = listCT;
                                break;
                            }
                        }
                    }

                    uniqueOrder.Add(foundRealCT);
                }

                folder.UniqueContentTypeOrder = uniqueOrder;
                folder.Update();
            }
        }

        private static void ClearFolderAllFolderMetadataDefaults(SPFolder folder)
        {
            MetadataDefaults listMetadataDefaults = new MetadataDefaults(folder.ParentWeb.Lists[folder.ParentListId]);
            listMetadataDefaults.RemoveAllFieldDefaults(folder);
            listMetadataDefaults.Update();
        }
    }
}
