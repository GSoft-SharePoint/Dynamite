﻿using System;
using System.Globalization;
using System.Xml.Linq;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Definition of a UserMultiField info
    /// </summary>
    public class UserMultiFieldInfo : BaseFieldInfoWithValueType<UserValueCollection>
    {
        /// <summary>
        /// Initializes a new UserMultiFieldFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        public UserMultiFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "UserMulti", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            // default person name
            this.ShowField = "ImnName";
            this.UserSelectionMode = UserFieldSelectionMode.PeopleOnly;
            this.UserSelectionScope = 0;   // default is 0 for no group constraint
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public UserMultiFieldInfo(XElement fieldSchemaXml)
            : base(fieldSchemaXml)
        {
            if (fieldSchemaXml.Attribute("ShowField") != null)
            {
                this.ShowField = fieldSchemaXml.Attribute("ShowField").Value;
            }

            if (fieldSchemaXml.Attribute("UserSelectionMode") != null)
            {
                this.UserSelectionMode = fieldSchemaXml.Attribute("UserSelectionMode").Value == UserFieldSelectionMode.PeopleAndGroups.ToString() ?
                    UserFieldSelectionMode.PeopleAndGroups : UserFieldSelectionMode.PeopleOnly;
            }

            if (fieldSchemaXml.Attribute("UserSelectionScope") != null)
            {
                this.UserSelectionScope = int.Parse(fieldSchemaXml.Attribute("UserSelectionScope").Value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// User profile property that will be shown
        /// </summary>
        public string ShowField { get; set; }

        /// <summary>
        /// Selection mode can be PeopleOnly or PeopleAndGroups
        /// </summary>
        public UserFieldSelectionMode UserSelectionMode { get; set; }

        /// <summary>
        /// The id of the group from which we want people to select people
        /// </summary>
        public int UserSelectionScope { get; set; }

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
            baseFieldSchema.Add(new XAttribute("List", "UserInfo"));
            baseFieldSchema.Add(new XAttribute("ShowField", this.ShowField));
            baseFieldSchema.Add(new XAttribute("UserSelectionMode", this.UserSelectionMode));
            baseFieldSchema.Add(new XAttribute("UserSelectionScope", this.UserSelectionScope));

            return baseFieldSchema;
        }
    }
}
