namespace Innofactor.Xrm.Utils.Common.Interfaces
{
    using System;
    using Innofactor.Xrm.Utils.Common.Misc;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Plugin specific extensions to IContainable
    /// </summary>
    public interface IPluginExecutionContainer : IExecutionContainer
    {
        /// <summary>
        /// Gets instance of the <see cref="IPluginExecutionContext" /> assosiated with current container
        /// </summary>
        IPluginExecutionContext Context
        {
            get;
        }

        EntitySet Entities
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
    }
}