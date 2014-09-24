﻿using System;
using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using Microsoft.SharePoint.Publishing;
using GSoft.Dynamite.ValueTypes;
using GSoft.Dynamite.Taxonomy;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition for a TaxonomyMulti field
    /// </summary>
    public class TaxonomyMultiFieldInfo : FieldInfo<TaxonomyValueCollection>
    {
        /// <summary>
        /// Initializes a new FieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Description resource key</param>
        public TaxonomyMultiFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "TaxonomyFieldTypeMulti", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
        }

        /// <summary>
        /// If true, the full parent-to-children path to the term will be rendered in the UI whenever
        /// a term is associated to this field
        /// </summary>
        public bool IsPathRendered { get; set; }

        /// <summary>
        /// If the associated TermSet is open and this value is true, then contributors will
        /// be able to "fill-in" taxonomy value
        /// </summary>
        public bool CreateValuesInEditForm { get; set; }

        /// <summary>
        /// Determines to which term set (and, optionally, which sub-term) the taxonomy column
        /// will be mapped, limiting the user's choices in the Edit Form's taxonomy picker.
        /// </summary>
        public TaxonomyContext TermStoreMapping { get; set; }

        /// <summary>
        /// The XML schema of the Taxonomy field
        /// </summary>
        public override XElement Schema
        {
            get
            {
                var schema = this.BasicFieldSchema;

                schema.Add(new XAttribute("Mult", "TRUE"));
                schema.Add(TaxonomyFieldInfo.TaxonomyFieldCustomizationSchema(Guid.NewGuid(), this.IsPathRendered, this.CreateValuesInEditForm));

                return schema;
            }
        }
    }
}
