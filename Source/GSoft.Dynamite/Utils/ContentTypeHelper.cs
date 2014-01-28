﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Helper class for managing content types.
    /// </summary>
    public class ContentTypeHelper
    {
        /// <summary>
        /// Ensures the SPContentType is in the collection. If not, it will be created and added.
        /// </summary>
        /// <param name="contentTypeCollection">The content type collection.</param>
        /// <param name="contentTypeId">The content type id.</param>
        /// <param name="contentTypeName">Name of the content type.</param>
        /// <returns><c>True</c> if it was added, else <c>False</c>.</returns>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPContentType EnsureContentType(SPContentTypeCollection contentTypeCollection, SPContentTypeId contentTypeId, string contentTypeName)
        {
            if (contentTypeCollection == null)
            {
                throw new ArgumentNullException("contentTypeCollection");
            }

            if (contentTypeId == null)
            {
                throw new ArgumentNullException("contentTypeId");
            }

            if (string.IsNullOrEmpty(contentTypeName))
            {
                throw new ArgumentNullException("contentTypeName");
            }

            SPList list = null;

            if (TryGetListFromContentTypeCollection(contentTypeCollection, out list))
            {
                // Make sure its not already in the list.
                var contentTypeInList = list.ContentTypes.Cast<SPContentType>().FirstOrDefault(ct => ct.Parent.Id == contentTypeId);
                if (contentTypeInList == null)
                {
                    // Can we add the content type to the list?
                    if (list.IsContentTypeAllowed(contentTypeId))
                    {
                        // Try to use the list's web's content type if it already exists
                        var contentTypeInWeb = list.ParentWeb.AvailableContentTypes[contentTypeId];

                        if (contentTypeInWeb != null)
                        {
                            // Add the web content type to the collection.
                            return list.ContentTypes.Add(contentTypeInWeb);
                        }
                        else
                        {
                            // Create the content type directly on the list
                            var newListContentType = new SPContentType(contentTypeId, contentTypeCollection, contentTypeName);
                            var returnedListContentType = list.ContentTypes.Add(newListContentType);
                            return returnedListContentType;
                        }
                    }
                }
                else
                {
                    return contentTypeInList;
                }
            }
            else
            {
                SPWeb web = null;
                if (TryGetWebFromContentTypeCollection(contentTypeCollection, out web))
                {
                    // Make sure its not already in ther web.
                    var contentTypeInWeb = web.ContentTypes[contentTypeId];
                    if (contentTypeInWeb == null)
                    {
                        // Add the content type to the collection.
                        var newWebContentType = new SPContentType(contentTypeId, contentTypeCollection, contentTypeName);
                        var returnedWebContentType = contentTypeCollection.Add(newWebContentType);
                        return returnedWebContentType;
                    }
                    else
                    {
                        return contentTypeInWeb;
                    }
                }

                // Case if there is no Content Types in the Web (e.g single SPWeb)
                var newContentType = new SPContentType(contentTypeId, contentTypeCollection, contentTypeName);
                var returnedContentType = contentTypeCollection.Add(newContentType);
                return returnedContentType;
            }

            return null;
        }

        /// <summary>
        /// Deletes the content type if not used.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="contentTypeId">The content type id.</param>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        /// <exception cref="Microsoft.SharePoint.SPContentTypeReadOnlyException">If the contentype is readonly.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void DeleteContentTypeIfNotUsed(SPContentTypeCollection collection, SPContentTypeId contentTypeId)
        {
            if (contentTypeId == null)
            {
                throw new ArgumentNullException("contentTypeId");
            }

            if (contentTypeId == null)
            {
                throw new ArgumentNullException("contentTypeId");
            }

            // Get the content type from the web.
            SPContentType contentType = collection[collection.BestMatch(contentTypeId)];

            // return false if the content type does not exist.
            if (contentType != null)
            {
                // Delete the content type if not used.
                this.DeleteContentTypeIfNotUsed(contentType);
            }
        }

        /// <summary>
        /// Deletes the content type if it has no SPContentTypeUsages.
        /// If it does, the content type will be deleted from the usages that are lists where it has no items.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void DeleteContentTypeIfNotUsed(SPContentType contentType)
        {
            // Find where the content type is being used.
            ICollection<SPContentTypeUsage> usages = SPContentTypeUsage.GetUsages(contentType);
            if (usages.Count <= 0)
            {
                // Delete unused content type.
                contentType.ParentWeb.ContentTypes.Delete(contentType.Id);
            }
            else
            {
                // Prepare the query to get all items in a list that uses the content type.
                SPQuery query = new SPQuery()
                {
                    Query = string.Concat(
                            "<Where><Eq>",
                                "<FieldRef Name='ContentTypeId'/>",
                                string.Format(CultureInfo.InvariantCulture, "<Value Type='Text'>{0}</Value>", contentType.Id),
                            "</Eq></Where>")
                };

                // Get the usages that are in a list.
                List<SPContentTypeUsage> listUsages = (from u in usages where u.IsUrlToList select u).ToList();
                foreach (SPContentTypeUsage usage in listUsages)
                {
                    // For a list usage, we get all the items in the list that use the content type.
                    SPList list = contentType.ParentWeb.GetList(usage.Url);
                    SPListItemCollection listItems = list.GetItems(query);

                    // if no items are found...
                    if (listItems.Count <= 0)
                    {
                        // Delete unused content type.
                        list.ContentTypes.Delete(contentType.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Ensures the SPField is in the content type. If not, it will be added and the content type updated.
        /// </summary>
        /// <param name="contentType">Type content type.</param>
        /// <param name="fieldInfo">The field info.</param>
        /// <returns>Null if the field does not exist, else the field is returned.</returns>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPField EnsureFieldInContentType(SPContentType contentType, FieldInfo fieldInfo)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }

            if (fieldInfo == null)
            {
                throw new ArgumentNullException("fieldInfo");
            }

            // Get the SPWeb from the contentType
            SPWeb web = contentType.ParentWeb;

            // We get from AvailableFields because we don't need to modify the field.
            SPField field = web.AvailableFields[fieldInfo.ID];

            if (field != null)
            {
                // Add the field to the content type and its children.
                AddFieldToContentType(contentType, field, true);
            }

            return field;
        }

        /// <summary>
        /// Ensures the SPFields are in the content type. If not, they will be added and the content type updated.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="fieldInfos">The field information.</param>
        /// <returns>IEnumerable of SPFields that where found.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public IEnumerable<SPField> EnsureFieldInContentType(SPContentType contentType, ICollection<FieldInfo> fieldInfos)
        {
            bool fieldWasAdded = false;
            List<SPField> fields = new List<SPField>();

            // For each field we want to add.
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                // We get the field from AvailableFields because we don't need to modify the field.
                SPField field = contentType.ParentWeb.AvailableFields[fieldInfo.ID];
                if (field != null)
                {
                    // We add it to the list of fields we got.
                    fields.Add(field);

                    // Then we add it to the content type without updating the content type.
                    if (AddFieldToContentType(contentType, field, false))
                    {
                        fieldWasAdded = true;
                    }
                }
            }

            if (fieldWasAdded)
            {
                // When One or more fields are added to the content type, we update the content type.
                contentType.Update(true);
            }

            return fields;
        }

        #region Private methods
        private static bool AddFieldToContentType(SPContentType contentType, SPField field, bool updateContentType)
        {
            // Create the field ref.
            SPFieldLink fieldOneLink = new SPFieldLink(field);
            if (contentType.FieldLinks[fieldOneLink.Id] == null)
            {
                // Field is not in the content type so we add it.
                contentType.FieldLinks.Add(fieldOneLink);

                // Update the content type.
                if (updateContentType)
                {
                    contentType.Update(true);
                }

                return true;
            }

            return false;
        }

        private static bool TryGetListFromContentTypeCollection(SPContentTypeCollection collection, out SPList list)
        {
            if (collection.Count > 0)
            {
                SPContentType first = collection[0];
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

        private static bool TryGetWebFromContentTypeCollection(SPContentTypeCollection collection, out SPWeb web)
        {
            if (collection.Count > 0)
            {
                SPContentType first = collection[0];
                if (first != null)
                {
                    if (first.ParentWeb != null)
                    {
                        web = first.ParentWeb;
                        return true;
                    }
                }  
            }

            web = null;
            return false;
        }
        #endregion
    }
}
