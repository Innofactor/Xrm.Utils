namespace Innofactor.Xrm.Utils.Common
{
    using System;
    using Innofactor.Xrm.Utils.Common.Extensions;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Innofactor.Xrm.Utils.Common.Misc;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Container object that helps to keep all objects and methods needed for CRM development in
    /// package easy to access and operate
    /// </summary>
    public class PluginContainer : IPluginExecutionContainer, IDisposable
    {
        private IPluginExecutionContext context;
        private Lazy<EntitySet> entities;
        private Lazy<ITracingService> logger;
        private Lazy<IOrganizationService> service;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginContainer" /> class.
        /// </summary>
        /// <param name="provider">
        /// Instance of <see cref="IServiceProvider" /> taken from MS CRM server
        /// </param>
        public PluginContainer(IServiceProvider provider)
        {
            // Reset values of entities, if they was already used —
            // this move will set fresh values for cached loggers and services
            Provider = provider;
            Context = Provider.Get<IPluginExecutionContext>();

            logger = new Lazy<ITracingService>(() => Provider.Get<ITracingService>());
            service = new Lazy<IOrganizationService>(() => Provider.Get<IOrganizationService>());

            entities = new Lazy<EntitySet>(() => new EntitySet(context));
        }

        /// <summary>
        /// </summary>
        public void Dispose()
        {
        }

        public EntitySet Entities =>
            entities.Value;

        public IServiceProvider Provider
        {
            get;
        }

        /// <summary>
        /// Gets instance of the <see cref="IPluginExecutionContext" /> assosiated with current container
        /// </summary>
        public IPluginExecutionContext Context
        {
            get;
        }

        /// <summary>
        /// Get instance of the <see cref="ILoggable" /> assosiated with current container
        /// </summary>
        public ITracingService Logger =>
            logger.Value;

        /// <summary>
        /// Gets instance of <see cref="IServicable" /> assosiated with current container
        /// </summary>
        public IOrganizationService Service =>
            service.Value;
    }
}