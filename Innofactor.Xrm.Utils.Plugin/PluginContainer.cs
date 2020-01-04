namespace Innofactor.Xrm.Utils.Common
{
    using System;
    using System.Dynamic;
    using Innofactor.Xrm.Utils.Common.Extensions;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Innofactor.Xrm.Utils.Common.Loggers;
    using Innofactor.Xrm.Utils.Common.Misc;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Container object that helps to keep all objects and methods needed for CRM development in
    /// package easy to access and operate
    /// </summary>
    public class PluginContainer : IPluginExecutionContainer, IDisposable
    {
        #region Private Fields

        private Lazy<EntitySet> entities;

        private Lazy<ILoggable> logger;

        private Lazy<IOrganizationService> service;

        #endregion Private Fields

        #region Public Constructors

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
            logger = new Lazy<ILoggable>(() => new CRMLogger(Provider.Get<ITracingService>()));
            service = new Lazy<IOrganizationService>(() => Provider.GetOrganizationService(Context.UserId));

            entities = new Lazy<EntitySet>(() => new EntitySet(Context));

            Values = new ExpandoObject();
            Values.IndentationLevel = 0;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets instance of the <see cref="IPluginExecutionContext" /> assosiated with current container
        /// </summary>
        public IPluginExecutionContext Context
        {
            get;
        }

        public EntitySet Entities =>
            entities.Value;

        /// <summary>
        /// Get instance of the <see cref="ILoggable" /> assosiated with current container
        /// </summary>
        public ILoggable Logger =>
            logger.Value;

        public IServiceProvider Provider
        {
            get;
        }

        /// <summary>
        /// Gets instance of <see cref="IServicable" /> assosiated with current container
        /// </summary>
        public IOrganizationService Service =>
            service.Value;

        public dynamic Values
        {
            get;
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// </summary>
        public void Dispose()
        {
        }

        #endregion Public Methods
    }
}