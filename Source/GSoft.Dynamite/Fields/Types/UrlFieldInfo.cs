﻿using System;
using System.Xml.Linq;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Definition of a UrlField info
    /// </summary>
    public class UrlFieldInfo : BaseFieldInfoWithValueType<UrlValue>
    {
        /// <summary>
        /// Initializes a new UrlFieldFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        public UrlFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "URL", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            // default format
            this.Format = UrlFieldFormat.Hyperlink;
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public UrlFieldInfo(XElement fieldSchemaXml)
            : base(fieldSchemaXml)
        {
            if (fieldSchemaXml.Attribute("Format") != null)
            {
                this.Format = fieldSchemaXml.Attribute("Format").Value == UrlFieldFormat.Image.ToString() ?
                    UrlFieldFormat.Image : UrlFieldFormat.Hyperlink;
            }
        }

        /// <summary>
        /// Format can be Hyperlink or Image
        /// </summary>
        public UrlFieldFormat Format { get; set; }

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
            baseFieldSchema.Add(new XAttribute("Format", this.Format.ToString()));

            return baseFieldSchema;
        }
    }
}
