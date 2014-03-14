﻿// -----------------------------------------------------------------------
// <copyright file="ICacheKey.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.Cache
{
    /// <summary>
    /// Defines contract for bilingual (french/english)
    /// content caching keys
    /// </summary>
    public interface ICacheKey
    {
        /// <summary>
        /// Get english key
        /// </summary>
        string InEnglish
        {
            get;
        }

        /// <summary>
        /// Get french key
        /// </summary>
        string InFrench
        {
            get;
        }
    }
}
