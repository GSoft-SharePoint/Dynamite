﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GSoft.Dynamite;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Globalization.Variations
{
    /// <summary>
    /// The VariationExpert interface.
    /// </summary>
    public interface IVariationExpert
    {
        /// <summary>
        /// The get variation url.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <returns>
        /// The <see cref="SPUrl"/>.
        /// </returns>
        Uri GetVariationRootUri(SPSite site);
    }
}
