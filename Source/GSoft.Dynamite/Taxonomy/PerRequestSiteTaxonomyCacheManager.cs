﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// The site taxonomy cache manager.
    /// </summary>
    public class PerRequestSiteTaxonomyCacheManager : ISiteTaxonomyCacheManager
    {
        private const string KeyPrefix = "PerRequestSiteTaxonomyCacheManager_";
        private ILogger log;

        /// <summary>
        /// Per-request taxonomy cache manager (using HttpContext.Items)
        /// </summary>
        /// <param name="log">Logging utility</param>
        public PerRequestSiteTaxonomyCacheManager(ILogger log)
        {
            this.log = log;
        }

        /// <summary>
        /// The get site taxonomy cache.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreName">
        /// The term store name.
        /// </param>
        /// <returns>
        /// The <see cref="SiteTaxonomyCache"/>.
        /// </returns>
        public SiteTaxonomyCache GetSiteTaxonomyCache(SPSite site, string termStoreName)
        {
            // No caching if outside HttpContext
            if (HttpContext.Current == null)
            {
                return new SiteTaxonomyCache(site, termStoreName);
            }

            string cacheKey = KeyPrefix + site.ID.ToString();

            // Create the Site Taxonomy Cache because it does not yet exist. No need for locking because
            // we only cache per-request using the HttpContext cache.
            if (HttpContext.Current.Items[cacheKey] == null)
            {
                var newTaxCache = new SiteTaxonomyCache(site, termStoreName);
                HttpContext.Current.Items[cacheKey] = newTaxCache;
            }

            // Return the existing Session
            return (SiteTaxonomyCache)HttpContext.Current.Items[cacheKey];
        }
    }
}
