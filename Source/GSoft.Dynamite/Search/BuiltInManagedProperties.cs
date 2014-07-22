﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Class to hold the constant for the already existing managed properties
    /// </summary>
    public static class BuiltInManagedProperties
    {
        /// <summary>
        /// The title managed property
        /// </summary>
        public static readonly string Title = "Title";

        /// <summary>
        /// The "Path" of the item is in reality his url
        /// </summary>
        public static readonly string Url = "Path";

        /// <summary>
        /// Managed properties for the ArticleStartDate field
        /// </summary>
        public static readonly string ArticleStartDate = "ArticleStartDateOWSDATE"; 
    }
}
