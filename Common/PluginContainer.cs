namespace Innofactor.Xrm.DevUtils.Common
{
    using System;
    using Innofactor.Xrm.DevUtils.Common.Constants;
    using Innofactor.Xrm.DevUtils.Common.Extensions;
    using Innofactor.Xrm.DevUtils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Container object that helps to keep all objects and methods needed for CRM development in
    /// package easy to access and operate
    /// </summary>
    public class PluginContainer : IPluginContainer, IDisposable
    {
        #region Private Fields

        private Lazy<Entity> completeEntity;
        private Lazy<IPluginExecutionContext> context;
        private Lazy<ITracingService> logger;
        private Lazy<Entity> postEntity;
        private Lazy<Entity> preEntity;
        private Lazy<IOrganizationService> service;
        private Lazy<Entity> targetEntity;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginContainer"/> class.
        /// </summary>
        /// <param name="provider">
        /// Instance of <see cref="IServiceProvider"/> taken from MS CRM server
        /// </param>
        public PluginContainer(IServiceProvider provider)
        {
            // Reset values of entities, if they was already used —
            // this move will set fresh values for cached loggers and services
            completeEntity = new Lazy<Entity>(() => GetCompleteEntity(provider));
            postEntity = new Lazy<Entity>(() => GetPostEntity(provider));
            preEntity = new Lazy<Entity>(() => GetPreEntity(provider));
            targetEntity = new Lazy<Entity>(() => GetTargetEntity(provider));
            service = new Lazy<IOrganizationService>(() => GetOrganizationService(provider));
            context = new Lazy<IPluginExecutionContext>(() => GetExecutionContext(provider));
            logger = new Lazy<ITracingService>(() => GetTracingService(provider));
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets or sets link to method that should execute main logic of the plugin. This method
        /// should have one input parameter of <see cref="PluginContainer"/> type, and should not have
        /// return value
        /// </summary>
        public Action<PluginContainer> Action
        {
            get;
            set;
        }

        /// <summary>
        /// All available entity information from context
        /// </summary>
        /// <returns>
        /// Complete <see cref="Entity"/>, merge of all atributes found on `target`,
        /// `preimage` and `postimage` entiies
        /// </returns>
        public Entity CompleteEntity =>
            completeEntity.Value;

        /// <summary>
        /// Gets instance of the <see cref="IPluginExecutionContext"/> assosiated with current container
        /// </summary>
        public IPluginExecutionContext Context =>
            context.Value;

        /// <summary>
        /// Get instance of the <see cref="ILoggable"/> assosiated with current container
        /// </summary>
        public ITracingService Logger =>
            logger.Value;

        /// <summary>
        /// Post image information from plugin execution context. The image name is hardcoded as `postimage`
        /// </summary>
        /// <returns>Post <see cref="Entity"/></returns>
        public Entity PostEntity =>
            postEntity.Value;

        /// <summary>
        /// Pre image information from plugin execution context. The image name is hardcoded as `preimage`
        /// </summary>
        /// <returns>Pre <see cref="Entity"/></returns>
        public Entity PreEntity =>
            preEntity.Value;

        /// <summary>
        /// Gets instance of <see cref="IServicable"/> assosiated with current container
        /// </summary>
        public IOrganizationService Service =>
            service.Value;

        /// <summary>
        /// Gets target information from plugin execution context
        /// </summary>
        /// <returns>Target <see cref="Entity"/></returns>
        public Entity TargetEntity =>
            targetEntity.Value;

        /// <summary>
        /// Gets or sets link to method that should validate plugin execution context. This method
        /// should have one input parameter of <see cref="IPluginExecutionContext"/> type, and should return
        /// <see cref="bool"/> value.
        /// </summary>
        public Predicate<IPluginExecutionContext> Validator
        {
            get;
            set;
        }
        
        #endregion Public Properties

        #region Public Methods

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Container's main entance point. Validater will be executed. In case of success, log will
        /// be initialized and main code will be invoked
        /// </summary>
        public virtual void Execute()
        {
            try
            {
                if (Action == null)
                {
                    throw new InvalidPluginExecutionException(string.Format("Main action is not set!"));
                }

                if (Validator != null && !Validator.Invoke(Context))    // Running context validator
                {
                    // In case if context is invalid, exitiong without notification
                    return;
                }

                // Invoking main action
                Action.Invoke(this);
            }
            catch (Exception ex)
            {
                Logger.Trace(ex);

                throw;
            }
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected static Entity GetCompleteEntity(IServiceProvider provider)
        {
            var result = default(Entity);

            var context = (IPluginExecutionContext)provider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains(ParameterName.Target) && context.InputParameters[ParameterName.Target] is Entity)
            {
                result = (Entity)context.InputParameters[ParameterName.Target];
            }

            if (context.PostEntityImages.Contains(ParameterName.PostImage))
            {
                var postImage = context.PostEntityImages[ParameterName.PostImage];

                if (result == null)
                {
                    result = postImage;
                }
                else
                {
                    result = this.Merge(result, postImage);
                }
            }

            if (context.PreEntityImages.Contains(ParameterName.PreImage))
            {
                var preImage = context.PreEntityImages[ParameterName.PreImage];

                if (result == null)
                {
                    result = preImage;
                }
                else
                {
                    result = this.Merge(result, preImage);
                }
            }

            if (result == null || result.Id.Equals(Guid.Empty))
            {
                var id = PluginHelper.GetEntityId(Context, false);
                if (!id.Equals(Guid.Empty))
                {
                    if (result == null)
                    {
                        result = new Entity(context.PrimaryEntityName, id);
                    }
                    else
                    {
                        result.Id = id;
                    }
                }
            }

            return result;
        }

        #endregion Protected Methods

        #region Private Methods

        private static IPluginExecutionContext GetExecutionContext(IServiceProvider provider) =>
            (IPluginExecutionContext)provider.GetService(typeof(IPluginExecutionContext));

        private static IOrganizationService GetOrganizationService(IServiceProvider provider) =>
            (IOrganizationService)provider.GetService(typeof(IOrganizationService));

        private static ITracingService GetTracingService(IServiceProvider provider) =>
            (ITracingService)provider.GetService(typeof(ITracingService));

        private static Entity GetPostEntity(IServiceProvider provider)
        {
            var context = (IPluginExecutionContext)provider.GetService(typeof(IPluginExecutionContext));

            if (context.PostEntityImages.Contains(ParameterName.PostImage) && context.PostEntityImages[ParameterName.PostImage] != null)
            {
                return context.PostEntityImages[ParameterName.PostImage];
            }
            return null;
        }

        private static Entity GetPreEntity(IServiceProvider provider)
        {
            var context = (IPluginExecutionContext)provider.GetService(typeof(IPluginExecutionContext));

            if (context.PreEntityImages.Contains(ParameterName.PreImage) && context.PreEntityImages[ParameterName.PreImage] != null)
            {
                return context.PreEntityImages[ParameterName.PreImage];
            }
            return null;
        }

        private Entity GetTargetEntity(IServiceProvider provider)
        {
            var context = (IPluginExecutionContext)provider.GetService(typeof(IPluginExecutionContext));

            try
            {
                if (context.InputParameters.Contains(ParameterName.Target) && context.InputParameters[ParameterName.Target] is Entity)
                {
                    return (Entity)context.InputParameters[ParameterName.Target];
                }
                else if (context.InputParameters.Contains(ParameterName.Target) && context.InputParameters[ParameterName.Target] is EntityReference)
                {
                    // In case of reference supplied — return entity will all attributes
                    var reference = (EntityReference)context.InputParameters[ParameterName.Target];
                    return Service.Retrieve(reference.LogicalName, reference.Id, new ColumnSet(true));
                }

                return null;
            }
            catch
            {
                // If any error happens — return null
                return null;
            }
        }

        #endregion Private Methods
    }
}