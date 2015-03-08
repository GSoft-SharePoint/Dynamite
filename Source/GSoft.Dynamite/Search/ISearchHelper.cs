using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Taxonomy;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Administration.Query;
using Microsoft.Office.Server.Search.Query;
using Microsoft.Office.Server.Search.Query.Rules;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Search service utilities
    /// </summary>
    public interface ISearchHelper
    {
        /// <summary>
        /// Gets the default search service application from a site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The search service application.</returns>
        SearchServiceApplication GetDefaultSearchServiceApplication(SPSite site);

        /// <summary>
        /// Get the service application by its name
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <returns>
        /// The search service application.
        /// </returns>
        SearchServiceApplication GetSearchServiceApplicationByName(string appName);

        /// <summary>
        /// Creates a site search scope if it doesn't exist yet
        /// </summary>
        /// <param name="site">The site collection</param>
        /// <param name="scopeName">The name of the search scope</param>
        /// <param name="displayGroupName">The scope's display group</param>
        /// <param name="searchPagePath">The scope's custom search page url (cannot be empty)</param>
        /// <returns>The search scope</returns>
        Scope EnsureSiteScope(SPSite site, string scopeName, string displayGroupName, string searchPagePath);

        /// <summary>
        /// Creates a farm-wide shared search scope
        /// </summary>
        /// <param name="site">The site collection of the context</param>
        /// <param name="scopeName">The name of the shared scope to create</param>
        /// <param name="displayGroupName">The search scope display group name</param>
        /// <param name="searchPagePath">Path to scope-specific search page</param>
        /// <returns>The newly created scope</returns>
        Scope EnsureSharedScope(SPSite site, string scopeName, string displayGroupName, string searchPagePath);

        /// <summary>
        /// Ensure a managed property in the search service application schema
        /// </summary>
        /// <param name="site">The context site</param>
        /// <param name="managedPropertyInfo">The managed property info</param>
        /// <returns>The managed property</returns>
        ManagedProperty EnsureManagedProperty(SPSite site, ManagedPropertyInfo managedPropertyInfo);

        /// <summary>The delete managed property.</summary>
        /// <param name="site">The site.</param>
        /// <param name="managedPropertyInfo">The managed property info.</param>
        void DeleteManagedProperty(SPSite site, ManagedPropertyInfo managedPropertyInfo);

        /// <summary>The ensure result type.</summary>
        /// <param name="site">The site.</param>
        /// <param name="resultType">The result type.</param>
        /// <returns>The <see cref="ResultItemType"/>.</returns>
        ResultItemType EnsureResultType(SPSite site, ResultTypeInfo resultType);

        /// <summary>The delete result type.</summary>
        /// <param name="site">The site.</param>
        /// <param name="resultType">The result type.</param>
        void DeleteResultType(SPSite site, ResultTypeInfo resultType);

        /// <summary>
        /// Gets the result source by name using the default search service application
        /// </summary>
        /// <param name="resultSourceName">Name of the result source.</param>
        /// <param name="site">The site collection.</param>
        /// <param name="scopeOwnerLevel">The level of the scope's owner.</param>
        /// <returns>
        /// The corresponding result source.
        /// </returns>
        ISource GetResultSourceByName(SPSite site, string resultSourceName, SearchObjectLevel scopeOwnerLevel);

        /// <summary>The ensure result source.</summary>
        /// <param name="contextSite">The context site.</param>
        /// <param name="resultSourceInfo">The result source info.</param>
        /// <returns>The <see cref="Source"/>.</returns>
        Source EnsureResultSource(SPSite contextSite, ResultSourceInfo resultSourceInfo);

        /// <summary>The delete result source.</summary>
        /// <param name="contextSite">The context site.</param>
        /// <param name="resultSourceInfo">The result source info.</param>
        void DeleteResultSource(SPSite contextSite, ResultSourceInfo resultSourceInfo);

        /// <summary>
        /// Deletes the result source.
        /// </summary>
        /// <param name="contextSite">Current site collection.</param>
        /// <param name="ssa">The search service application.</param>
        /// <param name="resultSourceName">Name of the result source.</param>
        /// <param name="level">The level.</param>
        void DeleteResultSource(SPSite contextSite, string resultSourceName, SearchObjectLevel level);

        /// <summary>
        /// Get all query rules matching the display name in the search level
        /// </summary>
        /// <param name="contextSite">The current site collection.</param>
        /// <param name="level">The search level.</param>
        /// <param name="displayName">The query rule display name.</param>
        /// <returns>A list of query rules</returns>
        ICollection<QueryRule> GetQueryRulesByName(SPSite contextSite, string displayName, SearchObjectLevel level);

        /// <summary>
        /// Creates a query rule object for the search level.
        /// </summary>
        /// <param name="site">The current site collection.</param>
        /// <param name="queryRuleMetadata">The query rule definition.</param>
        /// <param name="level">The search level object.</param>
        /// <returns>The new query rule object.</returns>
        QueryRule EnsureQueryRule(SPSite site, QueryRuleInfo queryRuleMetadata, SearchObjectLevel level);

        /// <summary>
        /// Delete all query rules corresponding to the display name
        /// </summary>
        /// <param name="site">The current site collection.</param>
        /// <param name="displayName">The query rule name.</param>
        /// <param name="level">The search level.</param>
        void DeleteQueryRule(SPSite site, string displayName, SearchObjectLevel level);

        /// <summary>
        /// Ensure a search best bet
        /// </summary>
        /// <param name="ssa">The search service application.</param>
        /// <param name="level">The search object level.</param>
        /// <param name="contextWeb">The SPWeb context.</param>
        /// <param name="title">The title of the best bet.</param>
        /// <param name="url">The url of the best bet.</param>
        /// <param name="description">The description of the best bet.</param>
        /// <param name="isVisualBestBet">True if it is a visual best bet. False otherwise.</param>
        /// <param name="deleteIfUnused">True if must be deleted if unused. False otherwise.</param>
        /// <returns>The best bet object.</returns>
        Microsoft.Office.Server.Search.Query.Rules.BestBet EnsureBestBet(SearchServiceApplication ssa, SearchObjectLevel level, SPWeb contextWeb, string title, Uri url, string description, bool isVisualBestBet, bool deleteIfUnused);

        /// <summary>
        /// Create a change query action for a Query Rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="queryTemplate">The search query template in KQL format</param>
        /// <param name="resultSourceId">The search result source Id</param>
        void CreateChangeQueryAction(QueryRule rule, string queryTemplate, Guid resultSourceId);

        /// <summary>
        /// Create a result block query action for a Query Rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="blockTitle">The result block Title</param>
        /// <param name="queryTemplate">The search query template in KQL format</param>
        /// <param name="resultSourceId">The search result source Id</param>
        /// <param name="routingLabel">A routing label for a content search WebPart</param>
        /// <param name="numberOfItems">The number of result to retrieve</param>
        void CreateResultBlockAction(QueryRule rule, string blockTitle, string queryTemplate, Guid resultSourceId, string routingLabel, string numberOfItems);

        /// <summary>
        /// Create a promoted link action for a a query rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="bestBetId">The bestBetIds</param>
        void CreatePromotedResultAction(QueryRule rule, Guid bestBetId);

        /// <summary>
        /// Creates a custom property rule
        /// </summary>
        /// <param name="resultTypeRule">The result type rule metadata</param>
        /// <returns>The created property rule</returns>
        PropertyRule CreateCustomPropertyRule(ResultTypeRuleInfo resultTypeRule);

        /// <summary>
        /// Add faceted navigation refiners for a taxonomy term and its reuses
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="navigationInfo">The faceted navigation configuration object</param>
        void AddFacetedRefinersForTerm(SPSite site, FacetedNavigationInfo navigationInfo);

        /// <summary>
        /// Deletes all refiners for the specified term and its reuses regardless previous configuration
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="term">The term info object</param>
        void RemoveFacetedRefinersForTerm(SPSite site, TermInfo term);
    }
}