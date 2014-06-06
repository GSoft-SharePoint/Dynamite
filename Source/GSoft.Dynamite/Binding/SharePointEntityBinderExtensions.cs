﻿using System.Collections.Generic;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding
{
    using System.Data;
    using System.Linq;

    /// <summary>
    /// Extensions to the basic functionality of ISharePointEntityBinder
    /// </summary>
    public static class SharePointEntityBinderExtensions
    {
        /// <summary>
        /// Extension method to convert a SPListItemCollection to a list of entities
        /// </summary>
        /// <typeparam name="T">The type of entities to return</typeparam>
        /// <param name="entityBinder">Client to the extension method</param>
        /// <param name="listItems">The list item collection</param>
        /// <returns>A list of T from the SPListItem</returns>
        public static IList<T> Get<T>(this ISharePointEntityBinder entityBinder, SPListItemCollection listItems) where T : new()
        {
            var returnList = new List<T>();

            if (listItems.Count > 0)
            {
                var table = listItems.GetDataTable();
                var rows = table.AsEnumerable();

                foreach (var dataRow in rows)
                {
                    returnList.Add(entityBinder.Get<T>(dataRow, listItems.Fields, listItems.List.ParentWeb));
                }
            }
          
            return returnList;
        }
    }
}
