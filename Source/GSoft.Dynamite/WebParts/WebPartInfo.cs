﻿using System;
using System.Web.UI.WebControls.WebParts;

namespace GSoft.Dynamite.WebParts
{
    /// <summary>
    /// Definition of a WebPart
    /// </summary>
    public class WebPartInfo
    {
        private WebPart webpart;

        /// <summary>
        /// Initializes a new <see cref="WebPartInfo"/> instance
        /// </summary>
        /// <param name="name">The title of web part</param>
        /// <param name="zoneName">The name of zone in which the web part should be instantiated</param>
        /// <param name="id">Unique identifier oft he web part</param>
        public WebPartInfo(string name, string zoneName, string id)
        {
            this.Name = name;
            this.ZoneName = zoneName;
            this.Id = "g_" + id;
        }

        /// <summary>
        /// Title of the web part
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Unique identifier for the web part
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Name of the WebPartZone to which to add the web part
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// The WebPart object that should be provisioned
        /// </summary>
        public WebPart WebPart
        {
            get
            {
                return this.webpart;
            }

            set
            {
                this.webpart = value;

                // Update the title
                this.webpart.Title = this.Name;

                // Update the ID
                this.webpart.ID = this.Id;
            }    
        }
    }
}
