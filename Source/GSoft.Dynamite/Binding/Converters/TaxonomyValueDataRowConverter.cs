﻿using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint.Taxonomy;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint;
using System;

namespace GSoft.Dynamite.Binding.Converters
{
    using System.Linq;

    /// <summary>
    /// The converter for taxonomy fields.
    /// </summary>
    public class TaxonomyValueDataRowConverter : DataRowValueConverter
    {
        ITaxonomyService taxonomyService;

        /// <summary>
        /// Converter constructor with dependency injection
        /// </summary>
        /// <param name="taxonomyService">Taxonomy service utility</param>
        public TaxonomyValueDataRowConverter(ITaxonomyService taxonomyService)
        {
            this.taxonomyService = taxonomyService;
        }

        #region IConverter Members

        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public override object Convert(object value, DataRowConversionArguments arguments)
        {
            if (value == System.DBNull.Value)
            {
                return null;
            }

            TaxonomyValue convertedValue = null;

            var taxonomyFieldValue = value as TaxonomyFieldValue;

            if (taxonomyFieldValue == null)
            {
                var stringValue = value as string;
                if (!string.IsNullOrEmpty(stringValue))
                {
                    var fieldObject = arguments.FieldCollection.Cast<SPField>()
                        .FirstOrDefault(f => f.InternalName == arguments.ValueKey);

                    if (fieldObject != null)
                    {
                        taxonomyFieldValue = new TaxonomyFieldValue(fieldObject);
                        taxonomyFieldValue.PopulateFromLabelGuidPair(stringValue);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            if (taxonomyFieldValue != null && !string.IsNullOrEmpty(taxonomyFieldValue.TermGuid))
            {
                if (SPContext.Current != null)
                {
                    // Resolve the Term from the term store, because we want all Labels and we want to
                    // create the TaxonomyValue object with a label in the correct LCID (we want one with
                    // LCID = CurrentUICUlture.LCID
                    Term underlyingTerm = this.taxonomyService.GetTermForId(SPContext.Current.Site, new Guid(taxonomyFieldValue.TermGuid));

                    if (underlyingTerm != null)
                    {
                        convertedValue = new TaxonomyValue(underlyingTerm);
                    }
                }
                else
                {
                    // We don't have access to a SPContext (needed to use the TaxonomyService), so we need to 
                    // fall back on the non-UICulture-respecting TaxonomyValue constructor
                    convertedValue = new TaxonomyValue(taxonomyFieldValue);
                }
            }

            return convertedValue;
        }

        /// <summary>
        /// Converts the specified value back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public override object ConvertBack(object value, DataRowConversionArguments arguments)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            var term = value as TaxonomyValue;
            TaxonomyFieldValue newTaxonomyFieldValue = null;

            TaxonomyField taxonomyField = (TaxonomyField)arguments.FieldCollection.GetField(arguments.ValueKey);
            newTaxonomyFieldValue = new TaxonomyFieldValue(taxonomyField);

            var noteField = arguments.FieldCollection[taxonomyField.TextField];

            if (term != null)
            {
                string labelGuidPair = term.Label + "|" + term.Id;

                // PopulateFromLabelGuidPair takes care of looking up the WssId value and creating a new item in the TaxonomyHiddenList if needed.
                // Main taxonomy field value format: WssID;#Label
                // TODO - Make sure we support sub-level terms with format: WssID;#Label|RootTermGuid|...|ParentTermGuid|TermGuid
                // Reference: http://msdn.microsoft.com/en-us/library/ee567833.aspx
                newTaxonomyFieldValue.PopulateFromLabelGuidPair(labelGuidPair);

                // Must write associated note field as well as the main taxonomy field.
                // Note field value format: Label|Guid
                // Reference: http://nickhobbs.wordpress.com/2012/02/21/sharepoint-2010-how-to-set-taxonomy-field-values-programmatically/
                arguments.FieldValues[noteField.InternalName] = labelGuidPair;
            }
            else
            {
                // No taxonomy value, make sure to empty the note field as well
                arguments.FieldValues[noteField.InternalName] = null;
            }

            return newTaxonomyFieldValue;
        }

        #endregion
    }
}
