﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using GSoft.Dynamite.Globalization.Variations;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Helper class for managing SP Fields.
    /// </summary>
    public class FieldHelper : IFieldHelper
    {
        private readonly ITaxonomyHelper taxonomyHelper;
        private readonly IFieldSchemaHelper fieldSchemaHelper;

        /// <summary>
        /// Default constructor with dependency injection
        /// </summary>
        /// <param name="taxonomyHelper">The taxonomy helper</param>
        /// <param name="fieldSchemaHelper">Field schema builder</param>
        public FieldHelper(ITaxonomyHelper taxonomyHelper, IFieldSchemaHelper fieldSchemaHelper)
        {
            this.taxonomyHelper = taxonomyHelper;
            this.fieldSchemaHelper = fieldSchemaHelper;
        }

        /// <summary>
        /// Ensure a field
        /// </summary>
        /// <param name="fieldCollection">The field collection</param>
        /// <param name="fieldInfo">The field info configuration</param>
        /// <returns>The internal name of the field</returns>
        public SPField EnsureField(SPFieldCollection fieldCollection, IFieldInfo fieldInfo)
        {
            SPField field = this.fieldSchemaHelper.EnsureFieldFromSchema(fieldCollection, this.fieldSchemaHelper.SchemaForField(fieldInfo));

            // Set default value if any, ensure other FieldType-specific properties
            this.ApplyFieldTypeSpecificValues(fieldCollection, field, fieldInfo);

            // Updates the visibility of the field
            UpdateFieldVisibility(field, fieldInfo);

            return field;
        }

        /// <summary>
        /// Ensure a collection of fields
        /// </summary>
        /// <param name="fieldCollection">The field collection</param>
        /// <param name="fieldInfos">The field info configuration</param>
        /// <returns>The internal names of the field</returns>
        public IEnumerable<SPField> EnsureField(SPFieldCollection fieldCollection, ICollection<IFieldInfo> fieldInfos)
        {
            var createdFields = new List<SPField>();

            foreach (IFieldInfo fieldInfo in fieldInfos)
            {
                createdFields.Add(this.EnsureField(fieldCollection, fieldInfo));
            }

            return createdFields;
        }

        private void ApplyFieldTypeSpecificValues(SPFieldCollection fieldCollection, SPField field, IFieldInfo fieldInfo)
        {
            var asTaxonomyFieldInfo = fieldInfo as TaxonomyFieldInfo;
            var asTaxonomyMultiFieldInfo = fieldInfo as TaxonomyMultiFieldInfo;

            if (fieldInfo is TextFieldInfo
                || fieldInfo is NoteFieldInfo
                || fieldInfo is HtmlFieldInfo)
            {
                FieldInfo<string> stringBasedField = fieldInfo as FieldInfo<string>;

                if (!string.IsNullOrEmpty(stringBasedField.DefaultValue))
                {
                    field.DefaultValue = stringBasedField.DefaultValue;
                }
            }
            else if (asTaxonomyFieldInfo != null)
            {
                this.ApplyTaxonomyFieldValues(fieldCollection, field, asTaxonomyFieldInfo);
            }
            else if (asTaxonomyMultiFieldInfo != null)
            {
                this.ApplyTaxonomyMultiFieldValues(fieldCollection, field, asTaxonomyMultiFieldInfo);
            }

            // TODO: support other field types (DateTimeFieldInfo, UrlFieldInfo, ImageFieldInfo, etc.)
        }

        private void ApplyTaxonomyFieldValues(SPFieldCollection fieldCollection, SPField field, TaxonomyFieldInfo taxonomyFieldInfo)
        {
            // Apply the term set mapping (taxonomy picker selection context) for the column
            if (taxonomyFieldInfo.TermStoreMapping != null)
            {
                this.ApplyTermStoreMapping(fieldCollection, taxonomyFieldInfo, taxonomyFieldInfo.TermStoreMapping);
            }

            // Set the default value for the field
            if (taxonomyFieldInfo.DefaultValue != null)
            {
                this.taxonomyHelper.SetDefaultTaxonomyFieldValue(fieldCollection.Web, field as TaxonomyField, taxonomyFieldInfo.DefaultValue);
            }
        }

        private void ApplyTaxonomyMultiFieldValues(SPFieldCollection fieldCollection, SPField field, TaxonomyMultiFieldInfo taxonomyMultiFieldInfo)
        {
            // Apply the term set mapping (taxonomy picker selection context) for the column
            if (taxonomyMultiFieldInfo.TermStoreMapping != null)
            {
                this.ApplyTermStoreMapping(fieldCollection, taxonomyMultiFieldInfo, taxonomyMultiFieldInfo.TermStoreMapping);
            }

            // Set the default value for the field
            if (taxonomyMultiFieldInfo.DefaultValue != null)
            {
                this.taxonomyHelper.SetDefaultTaxonomyFieldMultiValue(fieldCollection.Web, field as TaxonomyField, taxonomyMultiFieldInfo.DefaultValue);
            }
        }

        private void ApplyTermStoreMapping(SPFieldCollection fieldCollection, IFieldInfo fieldInfo, TaxonomyContext taxonomyMappingContext)
        {
            // Get the term store default language for term set name
            var termStoreDefaultLanguageLcid = this.taxonomyHelper.GetTermStoreDefaultLanguage(fieldCollection.Web.Site);

            string termSubsetName = string.Empty;
            if (taxonomyMappingContext.TermSubset != null)
            {
                termSubsetName = taxonomyMappingContext.TermSubset.Label;
            }

            // Metadata mapping configuration
            SPList parentList = null;

            // Try to see if we're playing with a List-field collection or a Web-field collection context
            if (TryGetListFromFieldCollection(fieldCollection, out parentList))
            {
                // Exure this term set mapping on the List-specific field only
                this.taxonomyHelper.AssignTermSetToListColumn(
                            parentList,
                            fieldInfo.Id,
                            taxonomyMappingContext.Group.Name,
                            taxonomyMappingContext.TermSet.Labels[new CultureInfo(termStoreDefaultLanguageLcid)],
                            termSubsetName);
            }
            else
            {
                // Ensure this field accross the web (i.e. site column + all usages of the field accross all the web's lists)
                this.taxonomyHelper.AssignTermSetToSiteColumn(
                            fieldCollection.Web,
                            fieldInfo.Id,
                            taxonomyMappingContext.Group.Name,
                            taxonomyMappingContext.TermSet.Labels[new CultureInfo(termStoreDefaultLanguageLcid)],
                            termSubsetName);
            }
        }

        private static SPField UpdateFieldVisibility(SPField field, IFieldInfo fieldInfo)
        {
            if (field != null)
            {
                field.ShowInListSettings = !fieldInfo.IsHiddenInListSettings;
                field.ShowInDisplayForm = !fieldInfo.IsHiddenInDisplayForm;
                field.ShowInEditForm = !fieldInfo.IsHiddenInEditForm;
                field.ShowInNewForm = !fieldInfo.IsHiddenInNewForm;
                field.Update(true);
            }

            return field;
        }

        private static bool TryGetListFromFieldCollection(SPFieldCollection collection, out SPList list)
        {
            if (collection.Count > 0)
            {
                SPField first = collection[0];
                if (first != null)
                {
                    if (first.ParentList != null)
                    {
                        list = first.ParentList;
                        return true;
                    }
                }
            }

            list = null;
            return false;
        }
    }
}
