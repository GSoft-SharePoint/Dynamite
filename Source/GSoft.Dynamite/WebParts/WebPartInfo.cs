﻿using System;
using System.Web.UI.WebControls.WebParts;

namespace GSoft.Dynamite.WebParts
{
    /// <summary>
    /// Definition of a WebPart
    /// </summary>
    public class WebPartInfo
    {
        /// <summary>
        /// Initializes a new <see cref="WebPartInfo" /> instance
        /// </summary>
        /// <param name="zoneName">The name of zone in which the web part should be instantiated</param>
        /// <param name="webPart">The WebPart object that should be instantiated</param>
        public WebPartInfo(string zoneName, WebPart webPart)
        {
            this.WebPart = webPart;
            this.ZoneName = zoneName;
        }

        /// <summary>
        /// Name of the WebPartZone to which to add the web part
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// The WebPart object that should be provisioned
        /// </summary>
        public WebPart WebPart { get; set; }
    }
}
