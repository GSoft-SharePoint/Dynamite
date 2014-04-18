﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.SiteColumns;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Catalogs
{
    /// <summary>
    /// Helper class for Cross Site Publishing operations
    /// </summary>
    public class CatalogBuilder
    {
        private readonly ILogger logger;
        private readonly ListHelper listHelper;
        private readonly TaxonomyHelper taxonomyHelper;

        /// <summary>
        /// Default constructor with dependency injection
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="listHelper">The List Helper</param>
        /// <param name="taxonomyHelper">The Taxonomy Helper</param>
        public CatalogBuilder(ILogger logger, ListHelper listHelper, TaxonomyHelper taxonomyHelper)
        {
            this.logger = logger;
            this.listHelper = listHelper;
            this.taxonomyHelper = taxonomyHelper;
        }

        /// <summary>
        /// Set a SharePoint as a product catalog without navigation term associated
        /// Note: For more information, see PublishingCatalogUtility in Microsoft.SharePoint.Publishing
        /// </summary>
        /// <param name="list">The SharePoint list.</param>
        /// <param name="availableFields">List of internal field names that are available through the catalog.</param>
        /// <returns>The SharePoint list configured as a catalog.</returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableFields)
        {
            // TODO REFACTOR
            this.logger.Info("Start method 'SetListAsCatalog' for list: '{0}'", list.RootFolder.Url);

            // Add properties for catalog publishing on the root folder
            list.IndexedRootFolderPropertyKeys.Add("PublishingCatalogSettings");
            list.IndexedRootFolderPropertyKeys.Add("IsPublishingCatalog");

            // Allow anonymous access on the parentWeb
            list.ParentWeb.FirstUniqueAncestorWeb.AnonymousPermMask64 |= SPBasePermissions.AnonymousSearchAccessWebLists;

            // Break list inheritance for anonymous access
            list.BreakRoleInheritance(true, false);

            // Allow anonymous access on the list
            list.AnonymousPermMask64 |= SPBasePermissions.AnonymousSearchAccessList;

            var fieldList = new Collection<string>();

            // For fields name, you need to pass the internal name of the column directly followed by "OWSTEXT"
            foreach (var availableField in availableFields)
            {
                fieldList.Add("\"" + availableField + "\"");
            }

            var friendlyUrlFieldsProperty = string.Join(",", fieldList.ToArray());

            var rootFolder = list.RootFolder;
            rootFolder.Properties["IsPublishingCatalog"] = "True";
            rootFolder.Properties["PublishingCatalogSettings"] = "{\"FurlFields\":[" + friendlyUrlFieldsProperty + "],\"TaxonomyFieldMap\":[]}";

            rootFolder.Properties["vti_indexedpropertykeys"] = "UAB1AGIAbABpAHMAaABpAG4AZwBDAGEAdABhAGwAbwBnAFMAZQB0AHQAaQBuAGcAcwA=|SQBzAFAAdQBiAGwAaQBzAGgAaQBuAGcAQwBhAHQAYQBsAG8AZwA=|";

            rootFolder.Update();
            list.Update();

            return list;
        }

        /// <summary>
        /// Set a SharePoint as a product catalog with a taxonomy term for navigation.
        /// </summary>
        /// <param name="list">The SharePoint list.</param>
        /// <param name="availableFields">List of internal field names that are available through the catalog.</param>
        /// <param name="taxonomyFieldMap">The taxonomy field that will be used for navigation.</param>
        /// <returns>The SharePoint list configured as a catalog.</returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableFields, string taxonomyFieldMap)
        {
            // TODO : REFACTOR
            this.logger.Info("Start method 'SetListAsCatalog' for list: '{0}'", list.RootFolder.Url);

            var catalogList = this.SetListAsCatalog(list, availableFields);

            if (string.IsNullOrEmpty(taxonomyFieldMap))
            {
                return catalogList;
            }

            // Set current culture to be able to get the "Title" of the list
            Thread.CurrentThread.CurrentUICulture = new CultureInfo((int)list.ParentWeb.Language);

            // Format property
            var fields = new List<string>();
            var taxonomyField = list.Fields.GetFieldByInternalName(taxonomyFieldMap) as TaxonomyField;

            if (taxonomyField != null)
            {
                fields.Add("{\"FieldDisplayName\":\"" + taxonomyField.Title + "\"");
                fields.Add("\"FieldManagedPropertyName\":\"owstaxid" + taxonomyFieldMap + "\"");
                fields.Add("\"FieldId\":\"" + taxonomyField.Id + "\"");
                fields.Add("\"TermSetId\":\"" + taxonomyField.TermSetId + "\"");
                fields.Add("\"IsSelected\":true");
                fields.Add("\"TermStoreId\":\"" + taxonomyField.SspId + "\"");
                fields.Add("\"TermId\":\"" + Guid.Empty + "\"}");
            }

            var taxonomyFieldMapProperty = "\"TaxonomyFieldMap\":[" + string.Join(",", fields) + "]";
            var newCatalogSettings = catalogList.RootFolder.Properties["PublishingCatalogSettings"].ToString().Replace("\"TaxonomyFieldMap\":[]", taxonomyFieldMapProperty);
            catalogList.RootFolder.Properties["PublishingCatalogSettings"] = newCatalogSettings;
            catalogList.RootFolder.Update();
            list.Update();

            return list;
        }

        /// <summary>
        /// Method to process a Catalog Object and configures it accordingly
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="catalog">The catalog definition</param>
        public void ProcessCatalog(SPWeb web, Catalog catalog)
        {
            // Set current culture to be able to set the "Title" of the list
            Thread.CurrentThread.CurrentUICulture = new CultureInfo((int)web.Language);

            // Create the list if doesn't exists
            var list = this.listHelper.EnsureList(web, catalog);

            // Remove Item Content Type
            if (catalog.RemoveDefaultContentType)
            {
                // If content type is direct child of item, remove it
                var bestMatchItem = list.ContentTypes.BestMatch(SPBuiltInContentTypeId.Item);
                if (bestMatchItem.Parent == SPBuiltInContentTypeId.Item)
                {
                    list.ContentTypes.Delete(bestMatchItem);
                }
            }

            // Add All Content Types
            foreach (var contentTypeId in catalog.ContentTypeIds)
            {
                this.listHelper.AddContentType(list, contentTypeId);
            }

            // Add Fields Segments
            this.CreateSegments(list, catalog.Segments);

            // Set default values for Fields
            this.SetDefaultValues(list, catalog.FieldDisplaySettings);

            // Set Display Settings
            this.SetDisplaySettings(list, catalog.DefaultValues);

            // Set versioning settings
            if (catalog.HasDraftVisibilityType)
            {
                list.EnableModeration = true;
                list.DraftVersionVisibility = catalog.DraftVisibilityType;
                list.Update();
            }

            // Set the list as catalog with navigation term
            this.SetListAsCatalog(list, catalog.ManagedProperties, catalog.TaxonomyFieldMap);

            // Enable or disable ratings
            this.listHelper.SetRatings(list, catalog.RatingType, catalog.EnableRatings);

            // Set SecurityOption
            this.listHelper.SetWriteSecurity(list, catalog.WriteSecurity);

        }

        private void CreateSegments(SPList list, IList<SiteColumnField> segments)
        {
            if (segments != null)
            {
                foreach (var segment in segments)
                {
                    if (segment is TaxoField)
                    {
                        var taxonomySegment = segment as TaxoField;

                        // Create the column in the list
                        var taxonomyField = this.listHelper.CreateListTaxonomyField(list, taxonomySegment.InternalName, taxonomySegment.DisplayName, taxonomySegment.Description, taxonomySegment.Group, taxonomySegment.IsMultiple, taxonomySegment.IsOpen);

                        // Set required if true
                        if (taxonomySegment.IsRequired)
                        {
                            taxonomyField.Required = true;
                            taxonomyField.Update();
                        }

                        // Assign the termSet to the field with an anchor term if specified
                        this.taxonomyHelper.AssignTermSetToListColumn(list, taxonomyField.Id, taxonomySegment.TermSetGroupName, taxonomySegment.TermSetName, taxonomySegment.TermSubsetName);
                        this.logger.Info("TaxonomyField " + segment.InternalName + " successfully created!");
                    }
                    else if (segment is TextField)
                    {
                        var textSegment = segment as TextField;

                        // Create the column in the list
                        var textField = this.listHelper.CreateTextField(list, segment.InternalName, segment.DisplayName, segment.Description, segment.Group, textSegment.IsMultiline);

                        // Set required if true
                        if (textSegment.IsRequired)
                        {
                            textField.Required = true;
                            textField.Update();
                        }

                        this.logger.Info("TextField " + segment.InternalName + " successfully created!");
                    }
                } 
            }
        }

        private void SetDefaultValues(SPList list, IList<SiteColumnField> defaultValues)
        {
            if (defaultValues != null)
            {
                foreach (var defaultValue in defaultValues)
                {
                    var field = list.Fields.GetFieldByInternalName(defaultValue.InternalName);
                    if (field.GetType() == typeof(Microsoft.SharePoint.Taxonomy.TaxonomyField) && (defaultValue is TaxoField))
                    {
                        var taxonomyDefaultValue = defaultValue as TaxoField;
                        if (((Microsoft.SharePoint.Taxonomy.TaxonomyField)field).AllowMultipleValues)
                        {
                            this.taxonomyHelper.SetDefaultTaxonomyMultiValue(list.ParentWeb, field, taxonomyDefaultValue.TermSetGroupName, taxonomyDefaultValue.TermSetName, defaultValue.DefaultValues.ToArray());
                        }
                        else
                        {
                            this.taxonomyHelper.SetDefaultTaxonomyValue(list.ParentWeb, field, taxonomyDefaultValue.TermSetGroupName, taxonomyDefaultValue.TermSetName, defaultValue.DefaultValues.FirstOrDefault());
                        }
                    }
                    else if (field.GetType() == typeof(SPFieldText))
                    {
                        field.DefaultValue = defaultValue.DefaultValues.FirstOrDefault();
                        field.Update();
                    }
                    else
                    {
                        this.logger.Warn(string.Format(CultureInfo.InvariantCulture, "Field '{0}' of type '{1}' cannot be found.", defaultValue.InternalName, defaultValue.GetType().Name));
                    }
                } 
            }
        }

        private void SetDisplaySettings(SPList list, IList<SiteColumnField> displaySettings)
        {
            if (displaySettings != null)
            {
                foreach (var field in displaySettings)
                {
                    var listfield = list.Fields.GetFieldByInternalName(field.InternalName);
                    if (listfield != null)
                    {
                        listfield.ShowInDisplayForm = field.ShowInDisplayForm;
                        listfield.ShowInEditForm = field.ShowInEditForm;
                        listfield.ShowInListSettings = field.ShowInListSettings;
                        listfield.ShowInNewForm = field.ShowInNewForm;
                        listfield.ShowInVersionHistory = field.ShowInVersionHistory;
                        listfield.ShowInViewForms = field.ShowInViewForm;

                        listfield.Update();
                    }
                }

                list.Update();
            }
        }
    }
}
