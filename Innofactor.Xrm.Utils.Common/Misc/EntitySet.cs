namespace Innofactor.Xrm.Utils.Common.Misc
{
    using System;
    using System.Linq;
    using Innofactor.Xrm.Utils.Common.Constants;
    using Innofactor.Xrm.Utils.Common.Extensions;
    using Microsoft.Xrm.Sdk;

    public class EntitySet
    {
        #region Private Fields

        private readonly Lazy<Entity> complete;
        private readonly Lazy<Entity> post;
        private readonly Lazy<Entity> pre;
        private readonly Lazy<Entity> target;

        #endregion Private Fields

        #region Public Constructors

        public EntitySet(IPluginExecutionContext context)
        {
            complete = new Lazy<Entity>(() => GetCompleteEntity(context));
            post = new Lazy<Entity>(() => GetPostEntity(context));
            pre = new Lazy<Entity>(() => GetPreEntity(context));
            target = new Lazy<Entity>(() => GetTargetEntity(context));
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// All available entity information from context
        /// </summary>
        /// <returns>
        /// Complete <see cref="Entity" />, merge of all atributes found on `target`,
        /// `preimage` and `postimage` entiies
        /// </returns>
        public Entity Complete =>
            complete.Value;

        /// <summary>
        /// Post image information from plugin execution context. The image name is hardcoded as `postimage`
        /// </summary>
        /// <returns>Post <see cref="Entity" /></returns>
        public Entity Post =>
            post.Value;

        /// <summary>
        /// Pre image information from plugin execution context. The image name is hardcoded as `preimage`
        /// </summary>
        /// <returns>Pre <see cref="Entity" /></returns>
        public Entity Pre =>
            pre.Value;

        /// <summary>
        /// Gets target information from plugin execution context
        /// </summary>
        /// <returns>Target <see cref="Entity" /></returns>
        public Entity Target =>
            target.Value;

        #endregion Public Properties

        #region Protected Methods

        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected static Entity GetCompleteEntity(IPluginExecutionContext context)
        {
            var result = default(Entity);

            if (context.InputParameters.Contains(ParameterName.Target) && context.InputParameters[ParameterName.Target] is Entity)
            {
                result = (Entity)context.InputParameters[ParameterName.Target];
            }

            if (context.PostEntityImages.Keys.Count > 0)
            {
                var postImage = context.PostEntityImages[context.PostEntityImages.Keys.First()];

                if (result == null)
                {
                    result = postImage;
                }
                else
                {
                    result = result.Merge(postImage);
                }
            }

            if (context.PreEntityImages.Keys.Count > 0)
            {
                var preImage = context.PreEntityImages[context.PreEntityImages.Keys.First()];

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
                var id = context.GetEntityId();
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

        private static Entity GetPostEntity(IPluginExecutionContext context)
        {
            if (context.PostEntityImages.Keys.Count > 0 && context.PostEntityImages[context.PostEntityImages.Keys.First()] != null)
            {
                return context.PostEntityImages[context.PostEntityImages.Keys.First()];
            }

            return null;
        }

        private static Entity GetPreEntity(IPluginExecutionContext context)
        {
            if (context.PreEntityImages.Keys.Count > 0 && context.PreEntityImages[context.PostEntityImages.Keys.First()] != null)
            {
                return context.PreEntityImages[context.PostEntityImages.Keys.First()];
            }

            return null;
        }

        private static Entity GetTargetEntity(IPluginExecutionContext context)
        {
            try
            {
                if (context.InputParameters.Contains(ParameterName.Target) && context.InputParameters[ParameterName.Target] is Entity)
                {
                    return (Entity)context.InputParameters[ParameterName.Target];
                }
                else if (context.InputParameters.Contains(ParameterName.Target) && context.InputParameters[ParameterName.Target] is EntityReference)
                {
                    // In case of reference supplied — return entity no attributes
                    var reference = (EntityReference)context.InputParameters[ParameterName.Target];

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

        #endregion Private Methods
    }
}