﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Logging;
using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes DateTime-based values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class DateTimeValueWriter : BaseValueWriter<DateTime?>
    {
        /// <summary>
        /// Writes a string field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var typedFieldValue = (DateTime?)fieldValueInfo.Value;

            if (typedFieldValue.HasValue)
            {
                item[fieldValueInfo.FieldInfo.InternalName] = typedFieldValue.Value;
            }
            else
            {
                item[fieldValueInfo.FieldInfo.InternalName] = null;
            }            
        }
        
        /// <summary>
        /// Writes a boolean value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (DateTime?)fieldValueInfo.Value;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (defaultValue.HasValue)
            {
                field.DefaultValue = FormatLocalDateTimeString(defaultValue.Value);
            }
            else
            {
                field.DefaultValue = null;
            }
        }

        /// <summary>
        /// Writes a standard field value as an SPFolder's default value
        /// </summary>
        /// <param name="folder">The field for which we wish to update the default value</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (DateTime?)fieldValueInfo.Value;
            var list = folder.ParentWeb.Lists[folder.ParentListId];
            MetadataDefaults listMetadataDefaults = new MetadataDefaults(list);

            if (defaultValue.HasValue)
            {
                // Assume that DefaultValue we need to format as string is in Local time.
                // In SharePoint. it's important to store everything as a UTC-string.
                string dateString = FormatLocalDateTimeString(defaultValue.Value.ToUniversalTime());
                listMetadataDefaults.SetFieldDefault(folder.ServerRelativeUrl, fieldValueInfo.FieldInfo.InternalName, dateString);
            }
            else
            {
                listMetadataDefaults.RemoveFieldDefault(folder.ServerRelativeUrl, fieldValueInfo.FieldInfo.InternalName);
            }

            listMetadataDefaults.Update();      
        }

        private static string FormatLocalDateTimeString(DateTime dateTime)
        {
            return SPUtility.CreateISO8601DateTimeFromSystemDateTime(dateTime);
        }
    }
}