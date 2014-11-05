namespace GSoft.Dynamite.Security
{
    using System.Collections.Generic;

    using Microsoft.SharePoint;

    /// <summary>
    /// SharePoint User Helper
    /// </summary>
    public interface IUserHelper
    {
        /// <summary>
        /// Get user SharePoint groups membership (bypass AD groups in SharePoint groups)
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="userName">The user name.</param>
        /// <returns>List of groups.</returns>
        ICollection<string> GetUserSharePointGroups(SPWeb web, string userName);
    }
}