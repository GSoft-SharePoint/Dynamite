﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Taxonomy;
using Microsoft.Office.Server.Auditing;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Administration.Query;
using Microsoft.Office.Server.Search.Query;
using Microsoft.Office.Server.Search.Query.Rules;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.JSGrid;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;
using Source = Microsoft.Office.Server.Search.Administration.Query.Source;

namespace GSoft.Dynamite.Search
{
    using System.Globalization;
    using GSoft.Dynamite.Utils;

    /// <summary>
    /// Search service utilities
    /// </summary>
    public class SearchHelper : ISearchHelper
    {
        private readonly ILogger logger;
        private readonly ITaxonomyService taxonomyService;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="taxonomyService">The taxonomy service instance</param>
        public SearchHelper(ILogger logger, ITaxonomyService taxonomyService)
        {
            this.logger = logger;
            this.taxonomyService = taxonomyService;
        }

        /// <summary>
        /// Creates a site search scope if it doesn't exist yet
        /// </summary>
        /// <param name="site">The site collection</param>
        /// <param name="scopeName">The name of the search scope</param>
        /// <param name="displayGroupName">The scope\"s display group</param>
        /// <param name="searchPagePath">The scope\"s custom search page url (cannot be empty)</param>
        /// <returns>The search scope</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Scope EnsureSiteScope(SPSite site, string scopeName, string displayGroupName, string searchPagePath)
        {
            // remotescopes class retrieves information via search web service so we run this as the search service account
            RemoteScopes remoteScopes = new RemoteScopes(SPServiceContext.GetContext(site));

            // see if there is an existing scope
            Scope scope = remoteScopes.GetScopesForSite(new Uri(site.Url)).Cast<Scope>().FirstOrDefault(s => s.Name == scopeName);

            // only add if the scope doesn\"t exist already
            if (scope == null)
            {
                scope = remoteScopes.AllScopes.Create(scopeName, string.Empty, new Uri(site.Url), true, searchPagePath, ScopeCompilationType.AlwaysCompile);
            }

            // see if there is an existing display group         
            ScopeDisplayGroup displayGroup = remoteScopes.GetDisplayGroupsForSite(new Uri(site.Url)).Cast<ScopeDisplayGroup>().FirstOrDefault(d => d.Name == displayGroupName);

            // add if the display group doesn\"t exist
            if (displayGroup == null)
            {
                displayGroup = remoteScopes.AllDisplayGroups.Create(displayGroupName, string.Empty, new Uri(site.Url), true);
            }

            // add scope to display group if not already added
            if (!displayGroup.Contains(scope))
            {
                displayGroup.Add(scope);
                displayGroup.Update();
            }

            // optionally force a scope compilation so this is available immediately
            remoteScopes.StartCompilation();

            return scope;
        }

        /// <summary>
        /// Creates a farm-wide shared search scope
        /// </summary>
        /// <param name="site">The site collection of the context</param>
        /// <param name="scopeName">The name of the shared scope to create</param>
        /// <param name="displayGroupName">The search scope display group name</param>
        /// <param name="searchPagePath">Path to scope-specific search page</param>
        /// <returns>The newly created scope</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Scope EnsureSharedScope(SPSite site, string scopeName, string displayGroupName, string searchPagePath)
        {
            // remotescopes class retrieves information via search web service so we run this as the search service account
            RemoteScopes remoteScopes = new RemoteScopes(SPServiceContext.GetContext(site));

            // see if there is an existing scope
            Scope scope = remoteScopes.GetSharedScopes().Cast<Scope>().FirstOrDefault(s => s.Name == scopeName);

            // only add if the scope doesn\"t exist already
            if (scope == null)
            {
                scope = remoteScopes.AllScopes.Create(scopeName, string.Empty, null, true, searchPagePath, ScopeCompilationType.AlwaysCompile);
            }

            // see if there is an existing display group         
            ScopeDisplayGroup displayGroup = remoteScopes.GetDisplayGroupsForSite(new Uri(site.Url)).Cast<ScopeDisplayGroup>().FirstOrDefault(d => d.Name == displayGroupName);

            // add if the display group doesn\"t exist
            if (displayGroup == null)
            {
                displayGroup = remoteScopes.AllDisplayGroups.Create(displayGroupName, string.Empty, new Uri(site.Url), true);
            }

            // add scope to display group if not already added
            if (!displayGroup.Contains(scope))
            {
                displayGroup.Add(scope);
                displayGroup.Update();
            }

            // optionally force a scope compilation so this is available immediately
            remoteScopes.StartCompilation();

            return scope;
        }

        /// <summary>
        /// Gets the result source by name using the default application name:\"Search Service Application\".
        /// </summary>
        /// <param name="resultSourceName">Name of the result source.</param>
        /// <param name="site">The site collection.</param>
        /// <param name="owner">The owner.</param>
        /// <returns>
        /// The corresponding result source.
        /// </returns>
        public SourceRecord GetResultSourceByName(string resultSourceName, SPSite site, SearchObjectLevel owner)
        {
            var serviceApplicationOwner = new SearchObjectOwner(owner);

            var context = SPServiceContext.GetContext(site);
            var searchProxy = context.GetDefaultProxy(typeof(SearchServiceApplicationProxy)) as SearchServiceApplicationProxy;

            return searchProxy.GetResultSourceByName(resultSourceName, serviceApplicationOwner);
        }

        /// <summary>
        /// Ensure a result source
        /// </summary>
        /// <param name="contextSite">The context SPSite object</param>
        /// <param name="resultSourceInfo">The result source configuration object</param>
        /// <returns>The name of the result source</returns>
        public Source EnsureResultSource(SPSite contextSite, ResultSourceInfo resultSourceInfo)
        {
            Source resultSource = null;
            var updateMode = resultSourceInfo.UpdateMode;

            var sortCollection = new SortCollection();

            if (resultSourceInfo.SortSettings != null)
            {            
                foreach (KeyValuePair<string, SortDirection> sortSetting in resultSourceInfo.SortSettings)
                {
                    sortCollection.Add(sortSetting.Key, sortSetting.Value);
                }
            }

            var queryProperties = new QueryTransformProperties();
            queryProperties["SortList"] = sortCollection;

            // Get the search service application for the current site
            var searchServiceApplication = this.GetDefaultSearchServiceApplication(contextSite);
            if (searchServiceApplication != null)
            {
                if (updateMode.Equals(UpdateBehavior.OverwriteResultSource))
                {
                    resultSource = InnerEnsureResultSource(
                        searchServiceApplication, 
                        resultSourceInfo.Name, 
                        resultSourceInfo.Level, 
                        resultSourceInfo.SearchProvider, 
                        contextSite.RootWeb, 
                        resultSourceInfo.Query,
                        queryProperties, 
                        true, 
                        resultSourceInfo.IsDefaultResultSourceForOwner);
                }
                else
                {
                    resultSource = InnerEnsureResultSource(
                        searchServiceApplication, 
                        resultSourceInfo.Name, 
                        resultSourceInfo.Level, 
                        resultSourceInfo.SearchProvider, 
                        contextSite.RootWeb, 
                        resultSourceInfo.Query,
                        queryProperties, 
                        false, 
                        resultSourceInfo.IsDefaultResultSourceForOwner);

                    string searchQuery = string.Empty;

                    if (updateMode.Equals(UpdateBehavior.OverwriteQuery))
                    {
                        searchQuery = resultSourceInfo.Query;
                    }

                    if (updateMode.Equals(UpdateBehavior.AppendToQuery))
                    {
                        if (resultSource.QueryTransform != null)
                        {
                            var rgx = new Regex(resultSourceInfo.Query);
                            if (!rgx.IsMatch(resultSource.QueryTransform.QueryTemplate))
                            {
                                searchQuery = resultSource.QueryTransform.QueryTemplate + " " + resultSourceInfo.Query;
                            }
                        }
                        else
                        {
                            searchQuery = resultSourceInfo.Query;
                        }
                    }

                    if (updateMode.Equals(UpdateBehavior.RevertQuery))
                    {
                        if (resultSource.QueryTransform != null)
                        {
                            var rgx = new Regex(resultSourceInfo.Query);
                            searchQuery = rgx.Replace(resultSource.QueryTransform.QueryTemplate, string.Empty);
                        }
                    }

                    resultSource.CreateQueryTransform(queryProperties, searchQuery);
                    resultSource.Commit();
                }
            }

            return resultSource;
        }

        /// <summary>
        /// Get a result source object by name
        /// </summary>
        /// <param name="ssa">The search service application</param>
        /// <param name="resultSourceName">The result source name</param>
        /// <param name="level">The search object level</param>
        /// <param name="contextWeb">The web context</param>
        /// <returns>The source object</returns>
        public Source GetResultSourceByName(SearchServiceApplication ssa, string resultSourceName, SearchObjectLevel level, SPWeb contextWeb)
        {
            var federationManager = new FederationManager(ssa);
            var searchOwner = new SearchObjectOwner(level, contextWeb);

            var resultSource = federationManager.GetSourceByName(resultSourceName, searchOwner);

            return resultSource;
        }
        
        /// <summary>
        /// Get the service application by its name
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <returns>
        /// The search service application.
        /// </returns>
        public SearchServiceApplication GetDefaultSearchServiceApplication(string appName)
        {
            var searchService = new SearchService("OSearch15", SPFarm.Local);
            var searchApplication = from SearchServiceApplication sapp in searchService.SearchApplications
                                    where sapp.GetSearchApplicationDisplayName() == appName
                                    select sapp;

            var serviceApp = searchApplication.First();

            return serviceApp;
        }

        /// <summary>
        /// Gets the default search service application from a site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The search service application.</returns>
        public SearchServiceApplication GetDefaultSearchServiceApplication(SPSite site)
        {
            var context = SPServiceContext.GetContext(site);

            // Get the search service application proxy
            var searchProxy = context.GetDefaultProxy(typeof(SearchServiceApplicationProxy)) as SearchServiceApplicationProxy;

            // Get the search service application info object so we can find the Id of our Search Service App
            if (searchProxy != null)
            {
                var applicationInfo = searchProxy.GetSearchServiceApplicationInfo();

                // Get the application itself
                return SearchService.Service.SearchApplications.GetValue<SearchServiceApplication>(applicationInfo.SearchServiceApplicationId);
            }

            return null;
        }

        /// <summary>
        /// Deletes the result source.
        /// </summary>
        /// <param name="ssa">The search service application.</param>
        /// <param name="resultSourceName">Name of the result source.</param>
        /// <param name="level">The level.</param>
        /// <param name="contextWeb">The context web.</param>
        public void DeleteResultSource(SearchServiceApplication ssa, string resultSourceName, SearchObjectLevel level, SPWeb contextWeb)
        {
            var federationManager = new FederationManager(ssa);
            var searchOwner = new SearchObjectOwner(level, contextWeb);

            var resultSource = federationManager.GetSourceByName(resultSourceName, searchOwner);
            if (resultSource != null)
            {
                federationManager.RemoveSource(resultSource);
            }
        }

        /// <summary>
        /// Delete a result source
        /// </summary>
        /// <param name="contextSite">The context site collection</param>
        /// <param name="resultSourceInfo">The result source info object</param>
        public void DeleteResultSource(SPSite contextSite, ResultSourceInfo resultSourceInfo)
        {
            // Get the search service application for the current site
            var searchServiceApplication = this.GetDefaultSearchServiceApplication(contextSite);
            if (searchServiceApplication != null)
            {
                this.DeleteResultSource(searchServiceApplication, resultSourceInfo.Name, resultSourceInfo.Level, contextSite.RootWeb);
            }
        }

        /// <summary>
        /// Creates a query rule object for the search level.
        /// </summary>
        /// <param name="ssa">The search service application.</param>
        /// <param name="level">The search level object.</param>
        /// <param name="contextWeb">The SPWeb context.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="isActive">True if the query is active. False otherwise.</param>
        /// <param name="startDate">The query rule publishing start date.</param>
        /// <param name="endDate">The query rule publishing end date.</param>
        /// <returns>The new query rule object.</returns>
        public QueryRule CreateQueryRule(SearchServiceApplication ssa, SearchObjectLevel level, SPWeb contextWeb, string displayName, bool isActive, DateTime? startDate, DateTime? endDate)
        {
            var queryRuleManager = new QueryRuleManager(ssa);
            var searchOwner = new SearchObjectOwner(level, contextWeb);

            // Build the SearchObjectFilter
            var searchObjectFilter = new SearchObjectFilter(searchOwner);

            var rules = queryRuleManager.GetQueryRules(searchObjectFilter);

            return rules.CreateQueryRule(displayName, startDate, endDate, isActive);
        }

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
        public Microsoft.Office.Server.Search.Query.Rules.BestBet EnsureBestBet(SearchServiceApplication ssa, SearchObjectLevel level, SPWeb contextWeb, string title, Uri url, string description, bool isVisualBestBet, bool deleteIfUnused)
        {
            Microsoft.Office.Server.Search.Query.Rules.BestBet bestBet = null;
            var queryRuleManager = new QueryRuleManager(ssa);
            var searchOwner = new SearchObjectOwner(level, contextWeb);

            // Build the SearchObjectFilter
            var searchObjectFilter = new SearchObjectFilter(searchOwner);

            var bestBets = queryRuleManager.GetBestBets(searchObjectFilter);

            if (!bestBets.Contains(url))
            {
                bestBet = bestBets.CreateBestBet(title, url, description, isVisualBestBet, deleteIfUnused);
            }
            else
            {
                bestBet = bestBets[url];
            }

            return bestBet;
        }

        /// <summary>
        /// Ensure a managed property in the search service application schema
        /// </summary>
        /// <param name="site">The context site</param>
        /// <param name="managedPropertyInfo">The managed property info</param>
        /// <param name="overwrite">True to overwrite.False otherwise</param>
        /// <returns>The managed property</returns>
        public ManagedProperty EnsureManagedProperty(SPSite site, ManagedPropertyInfo managedPropertyInfo, bool overwrite)
        {
            ManagedProperty managedProperty = null;
            var mappingCollection = new MappingCollection();
            var ssa = this.GetDefaultSearchServiceApplication(site);
            var propertyName = managedPropertyInfo.Name;
            var propertyType = managedPropertyInfo.DataType;

            // Get the search schema
            var sspSchema = new Schema(ssa);
            var managedProperties = sspSchema.AllManagedProperties;

            if (managedProperties.Contains(propertyName))
            {
                var prop = managedProperties[propertyName];
                if (overwrite)
                {
                    if (prop.DeleteDisallowed)
                    {
                        this.logger.Warn("Delete is disallowed on the Managed Property {0}", propertyName);
                    }
                    else
                    {
                        prop.DeleteAllMappings();
                        prop.Delete();
                        managedProperty = managedProperties.Create(propertyName, propertyType);
                    }
                }           
            }
            else
            {
                managedProperty = managedProperties.Create(propertyName, propertyType);               
            }

            if (managedProperty != null)
            {
                // Configure the managed property
                managedProperty.Sortable = managedPropertyInfo.Sortable;
                managedProperty.SortableType = managedPropertyInfo.SortableType;
                managedProperty.Refinable = managedPropertyInfo.Refinable;
                managedProperty.Retrievable = managedPropertyInfo.Retrievable;
                managedProperty.RespectPriority = managedPropertyInfo.RespectPriority;
                managedProperty.Queryable = managedPropertyInfo.Queryable;
                managedProperty.Searchable = managedPropertyInfo.Searchable;
                managedProperty.HasMultipleValues = managedPropertyInfo.HasMultipleValues;
                managedProperty.FullTextIndex = managedPropertyInfo.FullTextIndex;
                managedProperty.Context = managedPropertyInfo.Context;
                managedProperty.SafeForAnonymous = managedPropertyInfo.SafeForAnonymous;

                // Ensure crawl properties mappings
                foreach (KeyValuePair<string, int> crawledProperty in managedPropertyInfo.CrawledProperties)
                {
                    // Get the crawled property
                    var cp = this.GetCrawledPropertyByName(site, crawledProperty.Key);
                    
                    if (cp != null)
                    {
                        // Create mapping information
                        var mapping = new Mapping
                        {
                            CrawledPropertyName = cp.Name,
                            CrawledPropset = cp.Propset,
                            ManagedPid = managedProperty.PID,
                            MappingOrder = crawledProperty.Value
                        };

                        if (!managedProperty.GetMappings().Contains(mapping))
                        {
                            mappingCollection.Add(mapping);
                        }
                        else
                        {
                            this.logger.Info("Mapping for managed property {0} and crawled property with name {1} is already exists", managedProperty.Name, crawledProperty);
                        }
                    }
                    else
                    {
                        this.logger.Info("Crawled property with name {0} not found!", crawledProperty);
                    }
                }

                // Apply mappings to the managed property
                if (mappingCollection.Count > 0)
                {
                    managedProperty.SetMappings(mappingCollection);
                }

                managedProperty.Update();
            }

            return managedProperty;
        }

        /// <summary>
        /// Delete a managed property from the search schema
        /// </summary>
        /// <param name="site">The context site</param>
        /// <param name="managedPropertyInfo">The managed property info</param>
        public void DeleteManagedProperty(SPSite site, ManagedPropertyInfo managedPropertyInfo)
        {
            var ssa = this.GetDefaultSearchServiceApplication(site);

            // Get the search schema
            var sspSchema = new Schema(ssa);
            var managedProperties = sspSchema.AllManagedProperties;

            if (managedProperties.Contains(managedPropertyInfo.Name))
            {
                var prop = managedProperties[managedPropertyInfo.Name];
                prop.DeleteAllMappings();
                prop.Delete();
            }
        }

        /// <summary>
        /// Get a crawled property by name
        /// </summary>
        /// <param name="site">The context site</param>
        /// <param name="crawledPropertyName">The crawl property name</param>
        /// <returns>The crawled property</returns>
        public CrawledProperty GetCrawledPropertyByName(SPSite site, string crawledPropertyName)
        {
            CrawledProperty crawledProperty = null;

            var ssa = this.GetDefaultSearchServiceApplication(site);
            
            // Get the search schema
            var sspSchema = new Schema(ssa);

            // Search in all categories
            foreach (var category in sspSchema.AllCategories)
            {
                foreach (var property in category.GetAllCrawledProperties())
                {
                    if (string.CompareOrdinal(property.Name, crawledPropertyName) == 0)
                    {
                        crawledProperty = property;
                    }
                }
            }

            return crawledProperty;
        }

        /// <summary>
        /// Delete all query rules corresponding to the display name
        /// </summary>
        /// <param name="ssa">The search service application.</param>
        /// <param name="level">The search level.</param>
        /// <param name="contextWeb">The SPWeb context.</param>
        /// <param name="displayName">The query rule name.</param>
        public void DeleteQueryRule(SearchServiceApplication ssa, SearchObjectLevel level, SPWeb contextWeb, string displayName)
        {
            // Get all query rules for this level
            var rules = GetQueryRules(ssa, level, contextWeb);

            var queryRuleCollection = new List<QueryRule>();

            if (rules.Contains(displayName))
            {
                queryRuleCollection = rules[displayName].ToList();
            }

            if (queryRuleCollection.Count > 0)
            {
                foreach (var queryRule in queryRuleCollection)
                {
                    rules.RemoveQueryRule(queryRule);
                }
            }
        }

        /// <summary>
        /// Get all query rules matching the display name in the search level
        /// </summary>
        /// <param name="ssa">The search service.</param>
        /// <param name="level">The search level.</param>
        /// <param name="contextWeb">The SPWeb context.</param>
        /// <param name="displayName">The query rule display name.</param>
        /// <returns>A list of query rules</returns>
        public ICollection<QueryRule> GetQueryRulesByName(SearchServiceApplication ssa, SearchObjectLevel level, SPWeb contextWeb, string displayName)
        {
            var queryRules = new List<QueryRule>();

            // Get all query rules for this level
            var rules = GetQueryRules(ssa, level, contextWeb);

            if (rules.Contains(displayName))
            {
                queryRules = rules[displayName].ToList();
            }

            return queryRules;
        }

        /// <summary>
        /// Create a change query action for a Query Rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="queryTemplate">The search query template in KQL format</param>
        /// <param name="resultSourceId">The search result source Id</param>
        public void CreateChangeQueryAction(QueryRule rule, string queryTemplate, Guid resultSourceId)
        {
            var queryAction = (ChangeQueryAction)rule.CreateQueryAction(QueryActionType.ChangeQuery);

            if (!string.IsNullOrEmpty(queryTemplate))
            {
                queryAction.QueryTransform.QueryTemplate = queryTemplate;
            }

            queryAction.QueryTransform.SourceId = resultSourceId;

            rule.Update();
        }

        /// <summary>
        /// Create a result block query action for a Query Rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="blockTitle">The result block Title</param>
        /// <param name="queryTemplate">The search query template in KQL format</param>
        /// <param name="resultSourceId">The search result source Id</param>
        /// <param name="routingLabel">A routing label for a content search WebPart</param>
        /// <param name="numberOfItems">The number of result to retrieve</param>
        public void CreateResultBlockAction(QueryRule rule, string blockTitle, string queryTemplate, Guid resultSourceId, string routingLabel, string numberOfItems)
        {
            var queryAction = (CreateResultBlockAction)rule.CreateQueryAction(QueryActionType.CreateResultBlock);

            queryAction.ResultTitle.DefaultLanguageString = blockTitle;

            if (!string.IsNullOrEmpty(queryTemplate))
            {
                queryAction.QueryTransform.QueryTemplate = queryTemplate;
            }

            queryAction.QueryTransform.SourceId = resultSourceId;

            if (!string.IsNullOrEmpty(routingLabel))
            {
                queryAction.ResultTableType = routingLabel;
            }

            if (!string.IsNullOrEmpty(numberOfItems))
            {
                queryAction.QueryTransform.OverrideProperties = new QueryTransformProperties();
                queryAction.QueryTransform.OverrideProperties["RowLimit"] = int.Parse(numberOfItems, CultureInfo.InvariantCulture);
                queryAction.QueryTransform.OverrideProperties["TotalRowsExactMinimum"] = int.Parse(numberOfItems, CultureInfo.InvariantCulture);
            }

            rule.Update();
        }

        /// <summary>
        /// Create a promoted link action for a a query rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="bestBetId">The bestBetIds</param>
        public void CreatePromotedResultAction(QueryRule rule, Guid bestBetId)
        {
            var queryAction = (AssignBestBetsAction)rule.CreateQueryAction(QueryActionType.AssignBestBet);

            queryAction.BestBetIds.Add(bestBetId);

            rule.Update();
        }

        /// <summary>
        /// Ensure a Result Type in a site collection
        /// </summary>
        /// <param name="site">The site collection</param>
        /// <param name="resultType">The result type info object</param>
        /// <returns>The result type item</returns>
        public ResultItemType EnsureResultType(SPSite site, ResultTypeInfo resultType)
        {
            ResultItemType resType = null;

            var ssa = this.GetDefaultSearchServiceApplication(site);
            var searchOwner = new SearchObjectOwner(SearchObjectLevel.SPSite, site.RootWeb);
            var resultSource = this.GetResultSourceByName(ssa, resultType.ResultSource.Name, resultType.ResultSource.Level, site.RootWeb);

            var resultTypeManager = new ResultItemTypeManager(this.GetDefaultSearchServiceApplication(site));
            var existingResultTypes = resultTypeManager.GetResultItemTypes(searchOwner, true);

            // Get the existing result type
            resType = existingResultTypes.FirstOrDefault(r => r.Name.Equals(resultType.Name));

            if (resType == null)
            {
                resType = new ResultItemType(searchOwner);

                resType.Name = resultType.Name;
                resType.SourceID = resultSource.Id;

                resType.DisplayTemplateUrl = resultType.DisplayTemplate.ItemTemplateTokenizedPath;
                var properties = resultType.DisplayProperties.Select(t => t.Name).ToArray();
                resType.DisplayProperties = string.Join(",", properties);
                resType.RulePriority = resultType.Priority;

                // Create rules
                var rules = 
                    resultType.Rules.Select(
                        this.CreateCustomPropertyRule)
                        .ToList();
                resType.Rules = new PropertyRuleCollection(rules);

                typeof(ResultItemType).GetProperty("OptimizeForFrequentUse")
                    .SetValue(resType, resultType.OptimizeForFrequenUse);

                // Add the result type
                resultTypeManager.AddResultItemType(resType);
            }
 
            return resType;
        }

        /// <summary>
        /// Delete a result type in the site collection
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="resultType">The result type object</param>
        public void DeleteResultType(SPSite site, ResultTypeInfo resultType)
        {
            ResultItemType resType = null;
    
            var searchOwner = new SearchObjectOwner(SearchObjectLevel.SPSite, site.RootWeb);
            var resultTypeManager = new ResultItemTypeManager(this.GetDefaultSearchServiceApplication(site));
            var existingResultTypes = resultTypeManager.GetResultItemTypes(searchOwner, true);

            // Get the existing result type
            resType = existingResultTypes.FirstOrDefault(r => r.Name.Equals(resultType.Name));

            if (resType != null)
            {
                resultTypeManager.DeleteResultItemType(resType);
            }  
        }

        /// <summary>
        /// Create a custom result type rule
        /// </summary>
        /// <param name="resultTypeRule">The result type rule info object</param>
        /// <returns>The property rule</returns>
        public PropertyRule CreateCustomPropertyRule(ResultTypeRuleInfo resultTypeRule)
        {
            var type = typeof(PropertyRuleOperator);
            var info = type.GetProperty("DefaultOperators", BindingFlags.NonPublic | BindingFlags.Static);
            var value = info.GetValue(null);
            var defaultOperators = (Dictionary<PropertyRuleOperator.DefaultOperator, PropertyRuleOperator>)value;

            var rule = new PropertyRule(resultTypeRule.PropertyName, defaultOperators[resultTypeRule.Operator])
            {
                PropertyValues = new List<string>(resultTypeRule.Values)
            };
            return rule;
        }

        /// <summary>
        /// Add faceted navigation refiners for a taxonomy term and its reuses
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="navigationInfo">The faceted navigation configuration object</param>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "We want lowercase here.")]
        public void AddFacetedRefinersForTerm(SPSite site, FacetedNavigationInfo navigationInfo)
        {
            // Get the term
            var termItem = this.taxonomyService.GetTermForId(site, navigationInfo.Term.Id);

            // Flag the term set to use faceted navigation
            termItem.TermSet.SetCustomProperty("_Sys_Facet_IsFacetedTermSet", "True");
            termItem.TermStore.CommitAll();

            var termList = new List<Term>();

            termList.Add(termItem);
            termList.AddRange(termItem.ReusedTerms);

            foreach (var term in termList)
            {
                var i = 0;
                var fullRefinementString = new List<string>();
                var refinementConfig = new List<string>();

                foreach (var refiner in navigationInfo.Refiners)
                {
                    refinementConfig.Add("\"_Sys_Facet_RefinerConfig" + i + "\"");
                    fullRefinementString.Add(refiner.RefinementString);

                    var fp = CultureInfo.InvariantCulture;

                    var sb = new List<string>
                    {
                        string.Format(fp, "\"sortBy\":{0}", Convert.ChangeType(refiner.SortBy, refiner.SortBy.GetTypeCode(), fp)),
                        string.Format(fp, "\"sortOrder\":{0}", Convert.ChangeType(refiner.SortOrder, refiner.SortOrder.GetTypeCode(), fp)),
                        string.Format(fp, "\"maxNumberRefinementOptions\":{0}", refiner.MaxNumberRefinementOptions),
                        string.Format(fp, "\"propertyName\":\"{0}\"", refiner.ManagedPropertyName),
                        string.Format(fp, "\"type\":\"{0}\"", refiner.RefinerType),
                        string.Format(fp, "\"displayTemplate\":\"{0}\"", refiner.DisplayTemplateJsLocation),
                        string.Format(fp, "\"displayName\":\"{0}\"", refiner.DisplayName),
                        string.Format(fp, "\"useDefaultDateIntervals\":{0}", refiner.UseDefaultDateIntervals.ToString().ToLowerInvariant()),
                        string.Format(fp, "\"aliases\":{0}", refiner.Alias),
                        string.Format(fp, "\"refinerSpecStringOverride\":\"{0}\"", refiner.RefinerSpecStringOverride.ToLowerInvariant()),
                        string.Format(fp, "\"intervals\":{0}", refiner.Intervals)
                    };

                    term.SetCustomProperty("_Sys_Facet_RefinerConfig" + i, "{" + string.Join(",", sb.ToArray()) + "}");

                    i++;
                }

                term.SetCustomProperty("_Sys_Facet_FullRefinementString", string.Join(",", fullRefinementString.ToArray()));
                term.SetCustomProperty("_Sys_Facet_RefinementConfig",  "[" + string.Join(",", refinementConfig.ToArray()) + "]");

                term.TermStore.CommitAll();
            }
        }

        /// <summary>
        /// Deletes all refiners for the specified term and its reuses regardless previous configuration
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="term">The term info object</param>
        public void RemoveFacetedRefinersForTerm(SPSite site, TermInfo term)
        {
            // Get the term
            var termItem = this.taxonomyService.GetTermForId(site, term.Id);

            var properties = new[] { "_Sys_Facet_FullRefinementString", "_Sys_Facet_RefinementConfig", "_Sys_Facet_RefinerConfig" };

            foreach (var prop in properties)
            {
                string tmp;
                termItem.CustomProperties.TryGetValue(prop, out tmp);
                if (tmp != null)
                {
                    termItem.DeleteCustomProperty(prop);
                }
            }

            termItem.TermStore.CommitAll();
        }

        /// <summary>
        /// Get all query rules for a search level.
        /// </summary>
        /// <param name="ssa">The search service.</param>
        /// <param name="level">The search object level.</param>
        /// <param name="contextWeb">The SPWeb context.</param>
        /// <returns>A query rule collection.</returns>
        private static QueryRuleCollection GetQueryRules(SearchServiceApplication ssa, SearchObjectLevel level, SPWeb contextWeb)
        {
            var queryRuleManager = new QueryRuleManager(ssa);
            var searchOwner = new SearchObjectOwner(level, contextWeb);

            return queryRuleManager.GetQueryRules(new SearchObjectFilter(searchOwner));
        }

        /// <summary>
        /// Ensure a search result source
        /// </summary>
        /// <param name="ssa">The search service application.</param>
        /// <param name="resultSourceName">The result source name</param>
        /// <param name="level">The search object level.</param>
        /// <param name="searchProvider">The search provider for the result source.</param>
        /// <param name="contextWeb">The SPWeb to retrieve the search context.</param>
        /// <param name="query">The search query in KQL format.</param>
        /// <param name="properties">Query properties.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="isDefaultResultSourceForOwner">Whether this result source will be flagged as the default for the current search owner</param>
        /// <returns>
        /// The result source.
        /// </returns>
        private static Source InnerEnsureResultSource(SearchServiceApplication ssa, string resultSourceName, SearchObjectLevel level, string searchProvider, SPWeb contextWeb, string query, QueryTransformProperties properties, bool overwrite, bool isDefaultResultSourceForOwner)
        {
            var federationManager = new FederationManager(ssa);
            var searchOwner = new SearchObjectOwner(level, contextWeb);

            var resultSource = federationManager.GetSourceByName(resultSourceName, searchOwner);

            if (resultSource != null && overwrite)
            {
                federationManager.RemoveSource(resultSource);
            }

            if (resultSource == null || overwrite)
            {
                resultSource = federationManager.CreateSource(searchOwner);
                resultSource.Name = resultSourceName;
                resultSource.ProviderId = federationManager.ListProviders()[searchProvider].Id;
                resultSource.CreateQueryTransform(properties, query);
                resultSource.Commit();

                if (isDefaultResultSourceForOwner)
                {
                    federationManager.UpdateDefaultSource(resultSource.Id, searchOwner);
                }
            }

            return resultSource;
        }
    }
}
