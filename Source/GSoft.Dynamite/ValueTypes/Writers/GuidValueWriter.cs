﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes Guid-based values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class GuidValueWriter : BaseValueWriter<Guid?>
    {
        /// <summary>
        /// Writes a string field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var typedFieldValue = (Guid?)fieldValueInfo.Value;

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
        /// Writes a Guid value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var withDefaultVal = (FieldInfo<Guid?>)fieldValueInfo.FieldInfo;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (withDefaultVal.DefaultValue.HasValue)
            {
                field.DefaultValue = withDefaultVal.DefaultValue.Value.ToString();
            }
            else
            {
                field.DefaultValue = null;
            }
        }

        /// <summary>
        /// Writes a standard field value as an SPFolder's default value
        /// </summary>
        /// <param name="folder">The folder for which we wish to update the column metadata defaults</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValuesToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            throw new NotImplementedException();
        }
    }
}