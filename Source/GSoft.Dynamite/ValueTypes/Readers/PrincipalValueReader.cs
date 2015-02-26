﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Fields;

namespace GSoft.Dynamite.ValueTypes.Readers
{
    /// <summary>
    /// Reads User-based field values
    /// </summary>
    public class PrincipalValueReader : BaseValueReader<PrincipalValue>
    {
        /// <summary>
        /// Reads a field value from a list item
        /// </summary>
        /// <param name="item">The list item we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The value extracted from the list item's field</returns>
        public override PrincipalValue ReadValueFromListItem(SPListItem item, string fieldInternalName)
        {
            var fieldValue = item[fieldInternalName];

            if (fieldValue != null)
            {
                var userFieldVal = (SPFieldUserValue)fieldValue;
                return new PrincipalValue(userFieldVal.User.ID);
            }

            return null;
        }

        /// <summary>
        /// Reads a field value from a list item version
        /// </summary>
        /// <param name="itemVersion">The list item version we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The ImageValue extracted from the list item's field</returns>
        public override PrincipalValue ReadValueFromListItemVersion(SPListItemVersion itemVersion, string fieldInternalName)
        {
            var fieldValue = itemVersion[fieldInternalName];

            if (fieldValue != null)
            {
                var userFieldVal = (SPFieldUserValue)fieldValue;
                return new PrincipalValue(userFieldVal.User.ID);
            }

            return null;
        }

        /// <summary>
        /// Reads a field value from a DataRow returned by a CAML query
        /// </summary>
        /// <param name="web">The context's web</param>
        /// <param name="dataRowFromCamlResult">The CAML-query-result data row we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field among the data row cells</param>
        /// <returns>The value extracted from the data row's corresponding cell</returns>
        public override PrincipalValue ReadValueFromCamlResultDataRow(SPWeb web, DataRow dataRowFromCamlResult, string fieldInternalName)
        {
            var fieldValue = dataRowFromCamlResult[fieldInternalName];

            if (fieldValue != null)
            {
                var userFieldVal = new SPFieldUserValue(web, fieldValue.ToString());
                return new PrincipalValue(userFieldVal.User.ID);
            }

            return null;
        }
    }
}