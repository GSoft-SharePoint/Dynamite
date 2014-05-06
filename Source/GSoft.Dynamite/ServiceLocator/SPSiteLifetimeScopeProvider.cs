﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Lifetime scope provider the help share state at the SPSite-level
    /// </summary>
    public class SPSiteLifetimeScopeProvider : SPLifetimeScopeProvider
    {
        /// <summary>
        /// Creates a new per-SPSite lifetime scope provider so that state can be shared
        /// throughout the app's lifetime on a per-site-collection basis.
        /// </summary>
        /// <param name="containerProvider">The current container provider</param>
        public SPSiteLifetimeScopeProvider(ISharePointContainerProvider containerProvider)
            : base(containerProvider)
        { 
        }

        /// <summary>
        /// Creates a new scope or returns the existing scope unique to the current SPSite.
        /// The parent scope of the new SPSite-bound scope should be the root application container.
        /// </summary>
        public override ILifetimeScope LifetimeScope
        {
            get 
            {
                // Throw exception if not in SPContext
                this.ThrowExceptionIfNotSPContext();

                // Parent scope of SPSite scope is the Root application container
                var parentScope = this.ContainerProvider.Current;
                var scopeKindTag = SPLifetimeTag.Site;
                var childScopePerSiteContainerUniqueKey = scopeKindTag + SPContext.Current.Site.ID;

                return this.ChildScopeFactory.GetChildLifeTimeScope(parentScope, scopeKindTag, childScopePerSiteContainerUniqueKey);
            }
        }

        /// <summary>
        /// Disposes a lifetime scope and all its children.
        /// This implementation should be empty because SPSite-bound scope should live
        /// as long as the application container.
        /// </summary>
        public override void EndLifetimeScope()
        {
            // Nothing to dispose, SPSite scope should live as long as the root application container
        }
    }
}
