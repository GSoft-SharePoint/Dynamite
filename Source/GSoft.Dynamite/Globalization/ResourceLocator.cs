﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Web;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Globalization
{
    /// <summary>
    /// Locates resource objects from either AppGlobalResources or from 14/Resources
    /// </summary>
    public class ResourceLocator : IResourceLocator
    {
        private readonly string[] _defaultResourceFileNames;

        /// <summary>
        /// Creates a new resource locator which will default to the provided
        /// resource file name.
        /// </summary>
        /// <param name="resourceFileConfigs">All registered implementations for IResourceLocatorConfig</param>
        public ResourceLocator(IEnumerable<IResourceLocatorConfig> resourceFileConfigs)
        {
            var resourceFiles = new string[] { };
            resourceFiles = resourceFileConfigs.Aggregate(resourceFiles, (current, config) => current.Union(config.ResourceFileKeys).ToArray());

            this._defaultResourceFileNames = resourceFiles;
        }

        /// <summary>
        /// Retrieves the resource object specified by the key
        /// </summary>
        /// <param name="resourceKey">The resource key</param>
        /// <returns>The resource in the current UI language</returns>
        public string Find(string resourceKey)
        {
            return this.Find(resourceKey, CultureInfo.CurrentUICulture.LCID);
        }
        
        /// <summary>
        /// Retrieves the resource object specified by the key and language
        /// </summary>
        /// <param name="resourceKey">The resource key</param>
        /// <param name="lcid">The LCID of the desired culture</param>
        /// <returns>The resource in the specified language</returns>
        public string Find(string resourceKey, int lcid)
        {
            string resourceValue = null;

            // Scan all the default resource files
            foreach (var fileName in this._defaultResourceFileNames)
            {
                resourceValue = this.Find(fileName, resourceKey, new CultureInfo(lcid));

                if (!string.IsNullOrEmpty(resourceValue) && !resourceValue.StartsWith("$Resources"))
                {
                    // exit as soon as you find the resource in one of the default files
                    break;
                }
            }

            return resourceValue;
        }

        /// <summary>
        /// Retrieves the resource object specified by the key and language
        /// </summary>
        /// <param name="resourceFileName">The name of to the resource file where the resource is located</param>
        /// <param name="resourceKey">The resource key</param>
        /// <returns>The resource in the specified language</returns>
        public string Find(string resourceFileName, string resourceKey)
        {
            return this.Find(resourceFileName, resourceKey, CultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// Retrieves the resource object specified by the key and language
        /// </summary>
        /// <param name="resourceFileName">The name of to the resource file where the resource is located</param>
        /// <param name="resourceKey">The resource key</param>
        /// <param name="lcid">The LCID of the desired culture</param>
        /// <returns>The resource in the specified language</returns>
        public string Find(string resourceFileName, string resourceKey, int lcid)
        {
            return this.Find(resourceFileName, resourceKey, new CultureInfo(lcid));
        }

        /// <summary>
        /// Retrieves the resource object specified by the key and culture
        /// </summary>
        /// <param name="resourceFileName">The name of to the resource file where the resource is located</param>
        /// <param name="resourceKey">The resource key</param>
        /// <param name="culture">The desired culture</param>
        /// <returns>The resource in the specified language</returns>
        public string Find(string resourceFileName, string resourceKey, CultureInfo culture)
        {
            string found = string.Empty;

            try
            {
                // First, attempt to find the resource in VirtualDir/AppGlobalResources
                found = HttpContext.GetGlobalResourceObject(resourceFileName, resourceKey, culture) as string;
            }
            catch (MissingManifestResourceException)
            {
                // Swallow the exception
            }

            if (string.IsNullOrEmpty(found))
            {
                // Second, look into the 14/Resources
                try
                {
                    found = SPUtility.GetLocalizedString("$Resources:" + resourceKey, resourceFileName, Convert.ToUInt32(culture.LCID));
                }
                catch (COMException)
                {
                    // Failed to access ambient SPRequest object constructor. Fail to locate resource silently.
                }
            }

            return found;
        }

        /// <summary>
        /// Get the resource string with dollar format
        /// </summary>
        /// <param name="resourceFileName">The resource file name</param>
        /// <param name="resourceKey">The resource key</param>
        /// <returns>The resource string for the key and filename</returns>
        public string GetResourceString(string resourceFileName, string resourceKey)
        {
            return this._defaultResourceFileNames.Contains(resourceFileName) ? string.Format("$Resources:{0},{1};", resourceFileName, resourceKey) : string.Empty;
        }

        /// <summary>
        /// Get the resource string with dollar format
        /// </summary>
        /// <param name="resourceKey">The resource key</param>
        /// <returns>The resource string for the key and filename</returns>
        public string GetResourceString(string resourceKey)
        {
            string resourceString = null;
            bool isFound = false;

            // Scan all the default resource files
            foreach (var fileName in this._defaultResourceFileNames)
            {
                var resourceValue = this.Find(fileName, resourceKey);

                if (!string.IsNullOrEmpty(resourceValue) && !resourceValue.StartsWith("$Resources") && isFound == false)
                {
                    resourceString = string.Format("$Resources:{0},{1};", fileName, resourceKey);
                    isFound = true;
                }
            }

            return resourceString;
        }
    }
}
