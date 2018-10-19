namespace Innofactor.Xrm.Utils.Common.Interfaces
{
    using Innofactor.Xrm.Utils.Common.Misc;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Plugin specific extensions to IContainable
    /// </summary>
    public interface IPluginExecutionContainer : IContainable
    {
        /// <summary>
        /// Gets instance of the <see cref="IPluginExecutionContext"/> assosiated with current container
        /// </summary>
        IPluginExecutionContext Context
        {
            get;
        }

        EntitySet Entities
        {
            get;
        }
    }
}