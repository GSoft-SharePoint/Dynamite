﻿using System;
using System.Xml.Linq;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Definition of a LookupMultiFieldInfo
    /// </summary>
    public class LookupMultiFieldInfo : FieldInfo<LookupValueCollection>
    {
        /// <summary>
        /// Initializes a new LookupMultiFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        public LookupMultiFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "LookupMulti", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            // default lookup displayed field
            this.ShowField = "Title";
            this.ListId = Guid.Empty;
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public LookupMultiFieldInfo(XElement fieldSchemaXml)
            : base(fieldSchemaXml)
        {
            if (fieldSchemaXml.Attribute("ShowField") != null)
            {
                this.ShowField = fieldSchemaXml.Attribute("ShowField").Value;
            }

            if (fieldSchemaXml.Attribute("List") != null)
            {
                this.ListId = Guid.Parse(fieldSchemaXml.Attribute("List").Value);
            }
        }

        /// <summary>
        /// The internal name of the field of which we want to see the value in the lookup
        /// </summary>
        public string ShowField { get; set; }

        /// <summary>
        /// The looked-up list identifier
        /// </summary>
        public Guid ListId { get; set; }

        /// <summary>
        /// Extends a basic XML schema with the field type's extra attributes
        /// </summary>
        /// <param name="baseFieldSchema">
        /// The basic field schema XML (Id, InternalName, DisplayName, etc.) on top of which 
        /// we want to add field type-specific attributes
        /// </param>
        /// <returns>The full field XML schema</returns>
        public override XElement Schema(XElement baseFieldSchema)
        {
            baseFieldSchema.Add(new XAttribute("Mult", "TRUE"));
            baseFieldSchema.Add(new XAttribute("List", "{" + this.ListId + "}"));
            baseFieldSchema.Add(new XAttribute("ShowField", this.ShowField));

            return baseFieldSchema;
        }
    }
}
