namespace Innofactor.Xrm.DevUtils.Common.Interfaces
{
    using Microsoft.Xrm.Sdk;
    using System;

    /// <summary>
    /// Plugin specific extensions to IContainable
    /// </summary>
    public interface IPluginContainer : IContainable
    {
        #region Public Properties

        /// <summary>
        /// Gets instance of the <see cref="IPluginExecutionContext"/> assosiated with current container
        /// </summary>
        IPluginExecutionContext Context
        {
            get;
        }

        /// <summary>
        /// Gets instance of initial <see cref="IServiceProvider"/> assosiated with current plugin instance.
        /// </summary>
        IServiceProvider Provider
        {
            get;
        }

        #endregion Public Properties
    }
}