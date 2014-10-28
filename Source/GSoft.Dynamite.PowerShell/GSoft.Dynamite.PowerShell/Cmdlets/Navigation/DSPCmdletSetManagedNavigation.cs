﻿using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Autofac;
using GSoft.Dynamite.Navigation;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;
using Microsoft.SharePoint.PowerShell;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Navigation
{
    /// <summary>
    /// Cmdlet for managed metadata navigation configuration
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DSPManagedNavigation")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable once InconsistentNaming
    public class DSPCmdletSetManagedNavigation : SPCmdlet
    {
        private XDocument configurationFile;

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The path to the file containing the navigation configuration or an XmlDocument object or XML string.", Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        /// <summary>
        /// The end processing.
        /// </summary>
        protected override void InternalEndProcessing()
        {
            var xml = this.InputFile.Read();
            this.configurationFile = xml.ToXDocument();

            // Get all webs nodes
            var webNodes = from webNode in this.configurationFile.Descendants("Web") select webNode;

            foreach (var webNode in webNodes)
            {
                var webUrl = webNode.Attribute("Url").Value;
                using (var site = new SPSite(webUrl))
                {
                    using (var web = site.OpenWeb())
                    {
                        // Get managed navigation node if it exists
                        var managedNavigationNode = webNode.Descendants("ManagedNavigation").SingleOrDefault();
                        if (managedNavigationNode != null)
                        {
                            using (var childScope = PowerShellContainer.BeginLifetimeScope(web))
                            {
                                var settings = new ManagedNavigationSettings(managedNavigationNode);
                                var navigationHelper = childScope.Resolve<INavigationHelper>();
                                navigationHelper.SetWebNavigationSettings(web, settings);
                            }
                        }
                    }
                }
            }

            base.InternalEndProcessing();
        }
    }
}
