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
        private Lazy<IPluginExecutionContext> context;
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
            context = new Lazy<IPluginExecutionContext>(() => provider.Get<IPluginExecutionContext>());
            logger = new Lazy<ITracingService>(() => provider.Get<ITracingService>());
            service = new Lazy<IOrganizationService>(() => provider.Get<IOrganizationService>());

            entities = new Lazy<EntitySet>(() => new EntitySet(context));
        }

        /// <summary>
        /// Gets or sets link to method that should execute main logic of the plugin. This method
        /// should have one input parameter of <see cref="PluginContainer" /> type, and should not have
        /// return value
        /// </summary>
        internal Action<PluginContainer> Action
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets link to method that should validate plugin execution context. This method
        /// should have one input parameter of <see cref="IPluginExecutionContext" /> type, and should return
        /// <see cref="bool" /> value.
        /// </summary>
        internal Predicate<IPluginExecutionContext> Validator
        {
            get;
            set;
        }

        /// <summary>
        /// </summary>
        public void Dispose()
        {
        }

        public EntitySet Entities =>
            entities.Value;

        /// <summary>
        /// Gets instance of the <see cref="IPluginExecutionContext" /> assosiated with current container
        /// </summary>
        public IPluginExecutionContext Context =>
            context.Value;

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

        /// <summary>
        /// Container's main entance point. Validater will be executed. In case of success, log will
        /// be initialized and main code will be invoked
        /// </summary>
        internal virtual void Execute()
        {
            try
            {
                if (Action == null)
                {
                    throw new InvalidPluginExecutionException(string.Format("Main action is not set!"));
                }

                if (Validator != null && !Validator.Invoke(Context)) // Running context validator
                {
                    // In case if context is invalid, exitiong without notification
                    return;
                }

                // Invoking main action
                Action.Invoke(this);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                throw;
            }
        }
    }
}