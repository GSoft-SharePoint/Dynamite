﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Fields;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// Definition for a list
    /// </summary>
    public class ListInfo : BaseTypeInfo
    {
        /// <summary>
        /// Private variables to exchange values between ListTemplate and ListTemplateId
        /// </summary>
        private SPListTemplateType listTemplateType;
        private int listTemplateId;

        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public ListInfo()
        {
            // Default value
            this.WriteSecurity = WriteSecurityOptions.AllUser;
            this.Overwrite = false;
            this.ListTemplate = SPListTemplateType.GenericList;

            this.ContentTypes = new List<ContentTypeInfo>();
            this.DefaultViewFields = new List<IFieldInfo>();
            this.FieldDefinitions = new List<IFieldInfo>();
        }

        /// <summary>
        /// Initializes a new ListInfo
        /// </summary>
        /// <param name="webRelativeUrl">The web-relative URL of the list</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        public ListInfo(Uri webRelativeUrl, string displayNameResourceKey, string descriptionResourceKey)
            : base(displayNameResourceKey, descriptionResourceKey, string.Empty)
        {
            this.WebRelativeUrl = webRelativeUrl;

            // Default value
            this.WriteSecurity = WriteSecurityOptions.AllUser;
            this.Overwrite = false;
            this.ListTemplate = SPListTemplateType.GenericList;

            this.ContentTypes = new List<ContentTypeInfo>();
            this.DefaultViewFields = new List<IFieldInfo>();
            this.FieldDefinitions = new List<IFieldInfo>();
        }

        /// <summary>
        /// Initializes a new ListInfo
        /// </summary>
        /// <param name="webRelativeUrl">The web-relative URL of the list</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        public ListInfo(string webRelativeUrl, string displayNameResourceKey, string descriptionResourceKey)
            : this(new Uri(webRelativeUrl, UriKind.Relative), displayNameResourceKey, descriptionResourceKey)
        {
        }

        /// <summary>
        /// Gets or sets the root folder URL.
        /// </summary>
        /// <value>
        /// The root folder URL.
        /// </value>
        public Uri WebRelativeUrl { get; set; }

        /// <summary>
        /// Gets or sets the list template identifier by using the ENUM class provided by SP2013.
        /// </summary>
        /// <value>
        /// The list template identifier.
        /// </value>
        public SPListTemplateType ListTemplate 
        {
            get
            {
                return this.listTemplateType;
            }

            set
            {
                this.listTemplateId = (int)value;
                this.listTemplateType = value;
            } 
        }

        /// <summary>
        /// Gets or sets the list template identifier by using a real ID.
        /// </summary>
        /// <value>
        /// The list template identifier.
        /// </value>
        public int ListTemplateId 
        {
            get
            {
                return this.listTemplateId;
            }

            set
            {
                this.listTemplateId = value;
                this.listTemplateType = SPListTemplateType.InvalidType;
            } 
        }

        /// <summary>
        /// Gets or sets a value indicating whether [overwrite].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [overwrite]; otherwise, <c>false</c>.
        /// </value>
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [remove default content type].
        /// </summary>
        /// <value>
        /// <c>true</c> if [remove default content type]; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveDefaultContentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [has draft visibility type].
        /// </summary>
        /// <value>
        /// <c>true</c> if [has draft visibility type]; otherwise, <c>false</c>.
        /// </value>
        public bool HasDraftVisibilityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the draft visibility.
        /// </summary>
        /// <value>
        /// The type of the draft visibility.
        /// </value>
        public DraftVisibilityType DraftVisibilityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable ratings].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable ratings]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableRatings { get; set; }

        /// <summary>
        /// Gets or sets the type of the rating.
        /// </summary>
        /// <value>
        /// The type of the rating.
        /// </value>
        public string RatingType { get; set; }

        /// <summary>
        /// Gets or sets the write security.
        /// 1 — All users can modify all items.
        /// 2 — Users can modify only items that they create.
        /// 4 — Users cannot modify any list item.
        /// </summary>
        /// <value>
        /// The write security.
        /// </value>
        public WriteSecurityOptions WriteSecurity { get; set; }

        /// <summary>
        /// Content types definitions. If content types are specified, content type management
        /// should be turned on in your list. If not content types are specified, the collection
        /// of FieldDefinitions should be used to add fields to your list.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow replacement of backing store for more flexible intialization of collection.")]
        public ICollection<ContentTypeInfo> ContentTypes { get; set; }

        /// <summary>
        /// Add the list to quick launch
        /// </summary>
        public bool AddToQuickLaunch { get; set; }

        /// <summary>
        /// Enable attachments on the list
        /// </summary>
        public bool EnableAttachements { get; set; }

        /// <summary>
        /// The default view fields for the list
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow replacement of backing store for more flexible intialization of collection.")]
        public ICollection<IFieldInfo> DefaultViewFields { get; set; }

        /// <summary>
        /// List field definitions. Use to override site column definitions that come from ContentTypeInfo.
        /// If no ContentTypes are specified, these definitions should be used to add columns directly on
        /// your custom list.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow replacement of backing store for more flexible intialization of collection.")]
        public ICollection<IFieldInfo> FieldDefinitions { get; set; }

        /// <summary>
        /// The content types available on the new button of the list for the root folder.
        /// The content types here need to exist on the list.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow replacement of backing store for more flexible intialization of collection.")]
        public ICollection<ContentTypeInfo> UniqueContentTypeOrder { get; set; }
    }
}