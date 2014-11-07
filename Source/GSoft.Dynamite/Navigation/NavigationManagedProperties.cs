﻿using Microsoft.SharePoint.Publishing;
﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Managed property names
    /// </summary>
    public class NavigationManagedProperties
    {
        /// <summary>
        /// Public constructor
        /// </summary>
        public NavigationManagedProperties()
        {
            this.FriendlyUrlRequiredProperties = new List<string>();
        }

        /// <summary>
        /// The title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The item language
        /// </summary>
        public string ItemLanguage { get; set; }

        /// <summary>
        /// The navigation managed property name
        /// </summary>
        public string Navigation { get; set; }

        /// <summary>
        /// The occurrence link location managed property name
        /// </summary>
        public string OccurrenceLinkLocation { get; set; }

        /// <summary>
        /// The friendly URL required properties
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow repalcement of backing store for more flexible initialization.")]
        public ICollection<string> FriendlyUrlRequiredProperties { get; set; }

        /// <summary>
        /// The result source name
        /// </summary>
        public string ResultSourceName { get; set; }

        /// <summary>
        /// The Catalog Item Content Type Id 
        /// </summary>
        public string CatalogItemId { get; set; }

        /// <summary>
        /// The Catalog Item Content Type Id 
        /// </summary>
        public string TargetItemId { get; set; }

        /// <summary>
        /// The value of the managed property
        /// </summary>
        public string OccurrenceLinkLocationValue { get; set; }

        /// <summary>
        /// The list of query properties 
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Change 'NavigationManagedProperties.queryProperties' to be read-only by removing the property setter.")]
        public IList<string> QueryProperties { get; set; }
    }
}
