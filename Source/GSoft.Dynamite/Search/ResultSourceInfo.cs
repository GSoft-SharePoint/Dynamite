﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Query;
using Microsoft.SharePoint.JSGrid;
using Microsoft.SqlServer.Server;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Definition of a search result source
    /// </summary>
    public class ResultSourceInfo
    {
        private string _searchProvider;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResultSourceInfo()
        {
            this.UpdateMode = UpdateBehavior.NoChangesIfAlreadyExists;
            this.SortSettings = new Dictionary<string, SortDirection>();
        }

        /// <summary>
        /// Name of the result source
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Level of the result source
        /// </summary>
        public SearchObjectLevel Level { get; set; }

        /// <summary>
        /// The sorting setting by field. The Key corresponds the field name.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable easier initialization of object.")]
        public IDictionary<string, SortDirection> SortSettings { get; set; }

        /// <summary>
        /// Set the update behavior for the result source
        /// </summary>
        public UpdateBehavior UpdateMode { get; set; }

        /// <summary>
        /// The KQL Query
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Whether this result source should be flagged as default result source
        /// when registered on a particular owner (site or search service app).
        /// </summary>
        public bool IsDefaultResultSourceForOwner { get; set; }

        /// <summary>
        /// The Search Provider
        /// </summary>
        public string SearchProvider
        {
            get { return this._searchProvider ?? (this._searchProvider = "Local SharePoint Provider"); }
            set { this._searchProvider = value; }
        }
    }
}
