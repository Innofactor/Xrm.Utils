namespace Innofactor.Xrm.DevUtils.Common.Interfaces
{
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Plugin specific extensions to IContainable
    /// </summary>
    public interface IPluginContainer : IContainable
    {
        /// <summary>
        /// Gets instance of the <see cref="IPluginExecutionContext"/> assosiated with current container
        /// </summary>
        IPluginExecutionContext Context
        {
            get;
        }
    }
}