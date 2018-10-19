namespace Innofactor.Xrm.Utils.Common.Misc
{
    using System;
    using System.Linq;
    using Innofactor.Xrm.Utils.Common.Constants;
    using Innofactor.Xrm.Utils.Common.Extensions;
    using Microsoft.Xrm.Sdk;

    public class EntityComplect
    {
        private readonly Lazy<Entity> target;
        private readonly Lazy<Entity> pre;
        private readonly Lazy<Entity> post;
        private readonly Lazy<Entity> complete;

        public EntityComplect(Lazy<IPluginExecutionContext> context)
        {
            complete = new Lazy<Entity>(() => GetCompleteEntity(context));
            post = new Lazy<Entity>(() => GetPostEntity(context));
            pre = new Lazy<Entity>(() => GetPreEntity(context));
            target = new Lazy<Entity>(() => GetTargetEntity(context));
        }

        /// <summary>
        /// All available entity information from context
        /// </summary>
        /// <returns>
        /// Complete <see cref="Entity"/>, merge of all atributes found on `target`,
        /// `preimage` and `postimage` entiies
        /// </returns>
        public Entity Complete =>
            complete.Value;

        /// <summary>
        /// Pre image information from plugin execution context. The image name is hardcoded as `preimage`
        /// </summary>
        /// <returns>Pre <see cref="Entity"/></returns>
        public Entity Pre =>
            pre.Value;

        /// <summary>
        /// Post image information from plugin execution context. The image name is hardcoded as `postimage`
        /// </summary>
        /// <returns>Post <see cref="Entity"/></returns>
        public Entity Post =>
            post.Value;

        /// <summary>
        /// Gets target information from plugin execution context
        /// </summary>
        /// <returns>Target <see cref="Entity"/></returns>
        public Entity Target =>
            target.Value;

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