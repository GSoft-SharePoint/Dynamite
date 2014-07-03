﻿namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// The locator accessor interface
    /// </summary>
    public interface ISharePointServiceLocatorAccessor
    {
        /// <summary>
        /// Service locator instance
        /// </summary>
        ISharePointServiceLocator ServiceLocatorInstance { get; }
    }
}
