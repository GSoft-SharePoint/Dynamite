﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GSoft.Dynamite.Events;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.Utils;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Helper class for managing Taxonomy.
    /// </summary>
    public class TaxonomyHelper : ITaxonomyHelper
    {
        private const string AssemblyFullName = "Microsoft.SharePoint.Taxonomy, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        private const string ClassFullName = "Microsoft.SharePoint.Taxonomy.TaxonomyItemEventReceiver";

        private readonly IEventReceiverHelper eventReceiverHelper;
        private readonly ITaxonomyService taxonomyService;

        /// <summary>
        /// Creates a taxonomy helper.
        /// </summary>
        /// <param name="eventReceiverHelper">An event receiver helper.</param>
        /// <param name="taxonomyService">The taxonomy service.</param>
        public TaxonomyHelper(IEventReceiverHelper eventReceiverHelper, ITaxonomyService taxonomyService)
        {
            this.eventReceiverHelper = eventReceiverHelper;
            this.taxonomyService = taxonomyService;
        }

        /// <summary>
        /// Applies a term store mapping to a SharePoint field
        /// </summary>
        /// <param name="site">The current site collection</param>
        /// <param name="field">The site or list column to map to the term store</param>
        /// <param name="columnTermStoreMapping">
        /// The term set or sub-term-specific anchor which will determine what's available in the field's taxonomy picker
        /// </param>
        public void AssignTermStoreMappingToField(SPSite site, SPField field, TaxonomyContext columnTermStoreMapping)
        {
            TaxonomySession session = new TaxonomySession(site);

            TermStore store = null;
            if (columnTermStoreMapping.TermStore == null)
            {
                store = session.DefaultSiteCollectionTermStore;
            }
            else
            {
                store = session.TermStores[columnTermStoreMapping.TermStore.Name];
            }

            Group termStoreGroup = null;
            if (columnTermStoreMapping.Group == null)
            {
                termStoreGroup = store.GetSiteCollectionGroup(site);
            }
            else
            {
                termStoreGroup = store.Groups[columnTermStoreMapping.Group.Name];
            }

            TaxonomyField taxoField = (TaxonomyField)field;

            if (columnTermStoreMapping.TermSubset != null)
            {
                InternalAssignTermSetToTaxonomyField(store, taxoField, termStoreGroup.Id, columnTermStoreMapping.TermSet.Id, columnTermStoreMapping.TermSubset.Id);
            }
            else
            {
                InternalAssignTermSetToTaxonomyField(store, taxoField, termStoreGroup.Id, columnTermStoreMapping.TermSet.Id, Guid.Empty);
            }
        }

        /// <summary>
        /// Assigns a term set to a site column.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreName">The name of the term store.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termStoreName, string termStoreGroupName, string termSetName, string termSubsetName)
        {
            if (web.Fields.Contains(fieldId))
            {
                TaxonomySession session = new TaxonomySession(web.Site);
                TermStore termStore = session.TermStores[termStoreName];
                TaxonomyField field = (TaxonomyField)web.Site.RootWeb.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupName, termSetName, termSubsetName);
                AssignTermSetToAllListUsagesOfSiteColumn(web.Site, termStore, fieldId, termStoreGroupName, termSetName, termSubsetName);
            }
        }

        /// <summary>
        /// Assigns a term set to a site column in the default site collection
        /// term store.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termStoreGroupName, string termSetName, string termSubsetName)
        {
            if (web.Fields.Contains(fieldId))
            {
                TaxonomySession session = new TaxonomySession(web.Site);
                TermStore termStore = session.DefaultSiteCollectionTermStore;
                TaxonomyField field = (TaxonomyField)web.Site.RootWeb.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupName, termSetName, termSubsetName);
                AssignTermSetToAllListUsagesOfSiteColumn(web.Site, termStore, fieldId, termStoreGroupName, termSetName, termSubsetName);
            }
        }

        /// <summary>
        /// Assigns a term set to a site column in the default term store from the site collection's reserved group
        /// term store.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termSetName, string termSubsetName)
        {
            if (web.Fields.Contains(fieldId))
            {
                TaxonomySession session = new TaxonomySession(web.Site);
                TermStore termStore = session.DefaultSiteCollectionTermStore;
                Group siteCollectionGroup = termStore.GetSiteCollectionGroup(web.Site);
                TaxonomyField field = (TaxonomyField)web.Site.RootWeb.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, siteCollectionGroup.Name, termSetName, termSubsetName);
                AssignTermSetToAllListUsagesOfSiteColumn(web.Site, termStore, fieldId, siteCollectionGroup.Name, termSetName, termSubsetName);
            }
        }

        /// <summary>
        /// Assigns a term set to a site column in the default site collection
        /// term store.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupId">The term store group identifier.</param>
        /// <param name="termSetId">The term set identifier.</param>
        /// <param name="termSubsetId">The ID of the term sub set the term is attached to.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, Guid termStoreGroupId, Guid termSetId, Guid termSubsetId)
        {
            if (web.Fields.Contains(fieldId))
            {
                var session = new TaxonomySession(web.Site);
                var termStore = session.DefaultSiteCollectionTermStore;
                var field = (TaxonomyField)web.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupId, termSetId, termSubsetId);
            }
        }

        /// <summary>
        /// Assigns a term set to a list column.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreName">The name of the term store.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToListColumn(SPList list, Guid fieldId, string termStoreName, string termStoreGroupName, string termSetName, string termSubsetName)
        {
            if (list.Fields.Contains(fieldId))
            {
                TaxonomySession session = new TaxonomySession(list.ParentWeb.Site);
                TermStore termStore = session.TermStores[termStoreName];
                TaxonomyField field = (TaxonomyField)list.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupName, termSetName, termSubsetName);
            }
        }

        /// <summary>
        /// Assigns a term set to a list column in the default site collection
        /// term store.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupId">The term store group identifier.</param>
        /// <param name="termSetId">The term set identifier.</param>
        /// <param name="termSubsetId">The ID of the term sub set the term is attached to.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToListColumn(SPList list, Guid fieldId, Guid termStoreGroupId, Guid termSetId, Guid termSubsetId)
        {
            if (list.Fields.Contains(fieldId))
            {
                var session = new TaxonomySession(list.ParentWeb.Site);
                var termStore = session.DefaultSiteCollectionTermStore;
                var field = (TaxonomyField)list.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupId, termSetId, termSubsetId);
            }
        }

        /// <summary>
        /// Assigns a global farm-wide term set to a list column
        /// term store.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToListColumn(SPList list, Guid fieldId, string termStoreGroupName, string termSetName, string termSubsetName)
        {
            if (list.Fields.Contains(fieldId))
            {
                TaxonomySession session = new TaxonomySession(list.ParentWeb.Site);
                TermStore termStore = session.DefaultSiteCollectionTermStore;
                TaxonomyField field = (TaxonomyField)list.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupName, termSetName, termSubsetName);
            }
        }

        /// <summary>
        /// Assigns a local site-collection-specific term set to a list column
        /// term store.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToListColumn(SPList list, Guid fieldId, string termSetName, string termSubsetName)
        {
            if (list.Fields.Contains(fieldId))
            {
                TaxonomySession session = new TaxonomySession(list.ParentWeb.Site);
                TermStore termStore = session.DefaultSiteCollectionTermStore;
                TaxonomyField field = (TaxonomyField)list.Fields[fieldId];
                Group siteCollectionGroup = termStore.GetSiteCollectionGroup(list.ParentWeb.Site);
                InternalAssignTermSetToTaxonomyField(termStore, field, siteCollectionGroup.Name, termSetName, termSubsetName);
            }
        }

        /// <summary>
        /// Ensures the taxonomy event receivers.
        /// </summary>
        /// <param name="eventReceivers">The event receivers definition collection.</param>
        /// <exception cref="System.ArgumentNullException">All null parameters.</exception>
        public void EnsureTaxonomyEventReceivers(SPEventReceiverDefinitionCollection eventReceivers)
        {
            if (eventReceivers == null)
            {
                throw new ArgumentNullException("eventReceivers");
            }

            // Check if the ItemAdding exists in the collection.
            bool hasItemAdding = this.eventReceiverHelper.EventReceiverDefinitionExist(eventReceivers, SPEventReceiverType.ItemAdding, AssemblyFullName, ClassFullName);
            if (!hasItemAdding)
            {
                // Add the ItemAdding event receiver.
                eventReceivers.Add(SPEventReceiverType.ItemAdding, AssemblyFullName, ClassFullName);
            }

            // Check if the ItemUpdating exists in the collection.
            bool hasItemUpdating = this.eventReceiverHelper.EventReceiverDefinitionExist(eventReceivers, SPEventReceiverType.ItemUpdating, AssemblyFullName, ClassFullName);
            if (!hasItemUpdating)
            {
                // Add the ItemUpdating event receiver.
                eventReceivers.Add(SPEventReceiverType.ItemUpdating, AssemblyFullName, ClassFullName);
            }
        }

        /// <summary>
        /// Changes the Enterprise Keywords setting on a list
        /// </summary>
        /// <remarks>To disable Enterprise Keywords, delete the field from the list manually.</remarks>
        /// <param name="list">The list</param>
        /// <param name="keywordsAsSocialTags">Whether the list's keywords should be used as MySite social tags</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void EnableListEnterpriseKeywordsSetting(SPList list, bool keywordsAsSocialTags)
        {
            Assembly taxonomyAssembly = Assembly.Load("Microsoft.SharePoint.Taxonomy, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");

            // Get an instance of internal class for the keyword association
            Type listFieldSettings = taxonomyAssembly.GetType("Microsoft.SharePoint.Taxonomy.MetadataListFieldSettings");

            // Pass the list to the internal class's constructor
            object listSettings = listFieldSettings.GetConstructor(new Type[] { typeof(SPList) }).Invoke(new object[] { list });

            // Get an instance of keyword property and set the boolean
            listFieldSettings.GetProperty("EnableKeywordsField", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(listSettings, true, null);
            listFieldSettings.GetProperty("EnableMetadataPromotion", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(listSettings, keywordsAsSocialTags, null);

            // Update the list
            listFieldSettings.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(listSettings, null);
        }

        /// <summary>
        /// Get the validated string for a Taxonomy Field
        /// </summary>
        /// <param name="web">Web to look for</param>
        /// <param name="fieldName">Field to search</param>
        /// <param name="termGroup">The term group</param>
        /// <param name="termSet">The term set</param>
        /// <param name="termLabel">The term label</param>
        /// <returns>The validated string.</returns>
        public string GetTaxonomyFieldValueValidatedString(SPWeb web, string fieldName, string termGroup, string termSet, string termLabel)
        {
            SPField field = web.Fields.GetFieldByInternalName(fieldName);

            TaxonomyValue term = this.taxonomyService.GetTaxonomyValueForLabel(web.Site, termGroup, termSet, termLabel);

            if (term != null)
            {
                // Must be exist in the Taxonomy Hidden List
                var taxonomyFieldValue = new TaxonomyFieldValue(field);
                taxonomyFieldValue.PopulateFromLabelGuidPair(TaxonomyItem.NormalizeName(term.Label) + "|" + term.Id);

                return taxonomyFieldValue.ValidatedString;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the term group by name.
        /// </summary>
        /// <param name="termStore">The term store.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <returns>
        /// The term group.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Group GetTermGroupByName(TermStore termStore, string groupName)
        {
            var originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = Language.English.Culture.LCID;
            var group = termStore.Groups[groupName];
            termStore.WorkingLanguage = originalWorkingLanguage;

            return group;
        }

        /// <summary>
        /// Gets the term set by name.
        /// </summary>
        /// <param name="termStore">The term store.</param>
        /// <param name="group">The term group.</param>
        /// <param name="termSetName">Name of the term set.</param>
        /// <returns>The term set.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public TermSet GetTermSetByName(TermStore termStore, Group group, string termSetName)
        {
            var originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = Language.English.Culture.LCID;
            var termSet = group.TermSets[termSetName];
            termStore.WorkingLanguage = originalWorkingLanguage;

            return termSet;
        }

        /// <summary>
        /// Get a taxonomy term set bu its id
        /// </summary>
        /// <param name="termStore">The term store</param>
        /// <param name="group">The taxonomy term group</param>
        /// <param name="id">The term set id</param>
        /// <returns>The taxonomy term set</returns>
        public TermSet GetTermSetById(TermStore termStore, Group group, Guid id)
        {
            var originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = Language.English.Culture.LCID;
            var termSet = group.TermSets[id];
            termStore.WorkingLanguage = originalWorkingLanguage;

            return termSet;
        }

        /// <summary>
        /// Get the default language of the term store
        /// </summary>
        /// <param name="site">The site</param>
        /// <returns>The LCID of the default language</returns>
        public int GetTermStoreDefaultLanguage(SPSite site)
        {
            var taxonomySession = new TaxonomySession(site);
            var termStore = taxonomySession.DefaultSiteCollectionTermStore;

            return termStore.DefaultLanguage;
        }

        #region Private Methods
        private static void AssignTermSetToAllListUsagesOfSiteColumn(SPSite site, TermStore termStore, Guid fieldId, string termStoreGroupName, string termSetName, string termSubsetName)
        {
            var listFieldsToUpdate = new List<TaxonomyField>();

            foreach (SPWeb oneWeb in site.AllWebs)
            {
                foreach (SPList oneList in oneWeb.Lists)
                {
                    foreach (SPField oneField in oneList.Fields)
                    {
                        if (oneField.Id == fieldId)
                        {
                            var oneTaxoField = oneField as TaxonomyField;
                            if (oneTaxoField != null)
                            {
                                listFieldsToUpdate.Add(oneTaxoField);
                            }
                        }
                    }
                }
            }

            // Can't update the fields while iterating over their parent collection, so gotta do it after
            foreach (TaxonomyField taxFieldToReconnect in listFieldsToUpdate)
            {
                InternalAssignTermSetToTaxonomyField(termStore, taxFieldToReconnect, termStoreGroupName, termSetName, termSubsetName);
            }
        }

        private static void InternalAssignTermSetToTaxonomyField(TermStore termStore, TaxonomyField field, string termStoreGroupName, string termSetName, string termSubsetName)
        {
            Group group = termStore.Groups[termStoreGroupName];
            TermSet termSet = group.TermSets[termSetName];

            // Connect to MMS
            field.SspId = termSet.TermStore.Id;
            field.TermSetId = termSet.Id;
            field.TargetTemplate = string.Empty;

            // Select a sub node of the termset to limit selection
            if (!string.IsNullOrEmpty(termSubsetName))
            {
                Term term = termSet.GetTerms(termSubsetName, true)[0];
                field.AnchorId = term.Id;
            }
            else
            {
                field.AnchorId = Guid.Empty;
            }

            field.Update();
        }

        private static void InternalAssignTermSetToTaxonomyField(TermStore termStore, TaxonomyField field, Guid termStoreGroupId, Guid termSetId, Guid termSubsetId)
        {
            Group group = termStore.Groups[termStoreGroupId];
            TermSet termSet = group.TermSets[termSetId];

            // Connect to MMS
            field.SspId = termSet.TermStore.Id;
            field.TermSetId = termSet.Id;
            field.TargetTemplate = string.Empty;

            // Select a sub node of the termset to limit selection
            field.AnchorId = Guid.Empty != termSubsetId ? termSubsetId : Guid.Empty;
            field.Update();
        }
        #endregion
    }
}
