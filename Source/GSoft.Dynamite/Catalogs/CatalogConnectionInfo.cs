﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Catalogs
{
    /// <summary>
    /// Definition for a catalog connection
    /// </summary>
    public class CatalogConnectionInfo
    {
        /// <summary>
        /// Initializes a new catalog connection metadata object
        /// </summary>
        /// <param name="catalog">The catalog metadata object</param>
        /// <param name="catalogTaxonomyManagedProperty">The managed property name</param>
        /// <param name="rewriteCatalogItemUrls">Whether catalog item URLs should be rewritten</param>
        /// <param name="isManualCatalogItemUrlRewriteTemplate">Whether the URL rewriting is done through a manual template</param>
        /// <param name="isReusedWithPinning">Whether the term set can be reused through </param>
        /// <param name="catalogItemUrlRewriteTemplate">The manual item URL rewriting template</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "5#", Justification = "This is a template to follow in order to format friendly-URLs.")]
        public CatalogConnectionInfo(
            CatalogInfo catalog,
            string catalogTaxonomyManagedProperty,
            bool rewriteCatalogItemUrls,
            bool isManualCatalogItemUrlRewriteTemplate,
            bool isReusedWithPinning,
            string catalogItemUrlRewriteTemplate)
        {
            this.Catalog = catalog;
            this.CatalogTaxonomyManagedProperty = catalogTaxonomyManagedProperty;
            this.RewriteCatalogItemUrls = rewriteCatalogItemUrls;
            this.IsManualCatalogItemUrlRewriteTemplate = isManualCatalogItemUrlRewriteTemplate;
            this.IsReusedWithPinning = isReusedWithPinning;
            this.CatalogItemUrlRewriteTemplate = catalogItemUrlRewriteTemplate;
        }

        /// <summary>
        /// The catalog to connect
        /// </summary>
        public CatalogInfo Catalog { get; private set; }

        /// <summary>
        /// Indicates if the friendly url should be displayed instead of pointing to the source catalog
        /// </summary>
        public bool RewriteCatalogItemUrls { get; private set; }

        /// <summary>
        /// Indicates if it is a manual URL definition
        /// </summary>
        public bool IsManualCatalogItemUrlRewriteTemplate { get; private set; }

        /// <summary>
        /// Indicates it the connection use a reused with pinning
        /// </summary>
        public bool IsReusedWithPinning { get; private set; }

        /// <summary>
        /// The friendly URL template
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "This is a template to follow in order to format friendly-URLs.")]
        public string CatalogItemUrlRewriteTemplate { get; private set; }

        /// <summary>
        /// The navigation search managed property used to categorize catalog items
        /// </summary>
        public string CatalogTaxonomyManagedProperty { get; private set; }

        /// <summary>
        /// The target web (i.e the publishing web)
        /// </summary>
        public SPWeb TargetWeb { get; set; }

        /// <summary>
        /// The source web (i.e the web that contains the catalog)
        /// </summary>
        public SPWeb SourceWeb { get; set; }
    }
}
