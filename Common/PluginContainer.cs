namespace Innofactor.Xrm.DevUtils.Common
{
    using System;
    using System.Linq;
    using Innofactor.Xrm.DevUtils.Common.Constants;
    using Innofactor.Xrm.DevUtils.Common.Extensions;
    using Innofactor.Xrm.DevUtils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Container object that helps to keep all objects and methods needed for CRM development in
    /// package easy to access and operate
    /// </summary>
    public class PluginContainer : IPluginContainer, IDisposable
    {
        private Lazy<Entity> completeEntity;
        private Lazy<Entity> postEntity;
        private Lazy<Entity> preEntity;
        private Lazy<Entity> targetEntity;
        private Lazy<IOrganizationService> service;
        private Lazy<IPluginExecutionContext> context;
        private Lazy<ITracingService> logger;

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
            context = new Lazy<IPluginExecutionContext>(() => provider.Get<IPluginExecutionContext>());
            logger = new Lazy<ITracingService>(() => provider.Get<ITracingService>());
            service = new Lazy<IOrganizationService>(() => provider.Get<IOrganizationService>());

            completeEntity = new Lazy<Entity>(() => GetCompleteEntity(context));
            postEntity = new Lazy<Entity>(() => GetPostEntity(context));
            preEntity = new Lazy<Entity>(() => GetPreEntity(context));
            targetEntity = new Lazy<Entity>(() => GetTargetEntity(context));
        }

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

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected static Entity GetCompleteEntity(Lazy<IPluginExecutionContext> context)
        {
            var result = default(Entity);

            if (context.Value.InputParameters.Contains(ParameterName.Target) && context.Value.InputParameters[ParameterName.Target] is Entity)
            {
                result = (Entity)context.Value.InputParameters[ParameterName.Target];
            }

            if (context.Value.PostEntityImages.Keys.Count > 0)
            {
                var postImage = context.Value.PostEntityImages[context.Value.PostEntityImages.Keys.First()];

                if (result == null)
                {
                    result = postImage;
                }
                else
                {
                    result = result.Merge(postImage);
                }
            }

            if (context.Value.PreEntityImages.Keys.Count > 0)
            {
                var preImage = context.Value.PreEntityImages[context.Value.PreEntityImages.Keys.First()];

                if (result == null)
                {
                    result = preImage;
                }
                else
                {
                    result = result.Merge(preImage);
                }
            }

            if (result == null || result.Id.Equals(Guid.Empty))
            {
                var id = context.Value.GetEntityId();
                if (!id.Equals(Guid.Empty))
                {
                    if (result == null)
                    {
                        result = new Entity(context.Value.PrimaryEntityName, id);
                    }
                    else
                    {
                        result.Id = id;
                    }
                }
            }

            return result;
        }

        private static Entity GetPostEntity(Lazy<IPluginExecutionContext> context)
        {
            if (context.Value.PostEntityImages.Keys.Count > 0 && context.Value.PostEntityImages[context.Value.PostEntityImages.Keys.First()] != null)
            {
                return context.Value.PostEntityImages[context.Value.PostEntityImages.Keys.First()];
            }
            return null;
        }

        private static Entity GetPreEntity(Lazy<IPluginExecutionContext> context)
        {
            if (context.Value.PreEntityImages.Keys.Count > 0 && context.Value.PreEntityImages[context.Value.PostEntityImages.Keys.First()] != null)
            {
                return context.Value.PreEntityImages[context.Value.PostEntityImages.Keys.First()];
            }
            return null;
        }

        private static Entity GetTargetEntity(Lazy<IPluginExecutionContext> context)
        {
            try
            {
                if (context.Value.InputParameters.Contains(ParameterName.Target) && context.Value.InputParameters[ParameterName.Target] is Entity)
                {
                    return (Entity)context.Value.InputParameters[ParameterName.Target];
                }
                else if (context.Value.InputParameters.Contains(ParameterName.Target) && context.Value.InputParameters[ParameterName.Target] is EntityReference)
                {
                    // In case of reference supplied — return entity no attributes
                    var reference = (EntityReference)context.Value.InputParameters[ParameterName.Target];

                    return new Entity(reference.LogicalName, reference.Id);
                }

                return null;
            }
            catch
            {
                // If any error happens — return null
                return null;
            }
        }
    }
}