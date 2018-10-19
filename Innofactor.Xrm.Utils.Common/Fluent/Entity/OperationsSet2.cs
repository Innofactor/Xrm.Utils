namespace Innofactor.Xrm.Utils.Common.Fluent.Entity
{
    using System;
    using System.Collections.Concurrent;
    using System.ServiceModel;
    using Innofactor.Xrm.Utils.Common.Extensions;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Metadata;

    public class OperationsSet2 : InformationBase
    {
        protected readonly string logicalName;

        private static readonly ConcurrentDictionary<string, string> PrimaryIdAttributes = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, string> PrimaryNameAttributes = new ConcurrentDictionary<string, string>();

        internal OperationsSet2(IExecutionContainer container, string logicalName)
            : base(container) =>
            this.logicalName = logicalName;

        /// <summary>
        /// Primary Id attribute of the entity
        /// </summary>
        public string PrimaryIdAttribute =>
           primaryIdAttribute.Value;

        /// <summary>
        /// Primary Name attribute of the entity
        /// </summary>
        public string PrimaryNameAttribute =>
            primaryNameAttribute.Value;

        private Lazy<string> primaryIdAttribute =>
            GetPrimaryIdAttribute();

        private Lazy<string> primaryNameAttribute =>
            GetPrimaryNameAttribute();

        public OperationsSet3 LinkedTo(Entity target) =>
            LinkedTo(target.ToEntityReference());

        public OperationsSet3 LinkedTo(EntityReference target) =>
            new OperationsSet3(container, logicalName, target);

        /// <summary>
        /// Get the primary id attribute of the target entity
        /// </summary>
        /// <remarks>Metadata request that is time consuming, but results are cached.</remarks>
        /// <returns></returns>
        private Lazy<string> GetPrimaryIdAttribute()
        {
            var result = string.Empty;

            if (PrimaryIdAttributes.ContainsKey(logicalName))
            {
                result = PrimaryIdAttributes[logicalName];
            }
            else
            {
                try
                {
                    var request = new RetrieveEntityRequest()
                    {
                        LogicalName = logicalName,
                        EntityFilters = EntityFilters.Entity,
                        RetrieveAsIfPublished = true
                    };

                    var response = (RetrieveEntityResponse)container.Service.Execute(request);

                    var metabase = response.EntityMetadata;

                    container.Logger.Log("Metadata retrieved");
                    if (metabase is EntityMetadata meta)
                    {
                        result = meta.PrimaryIdAttribute;
                        PrimaryIdAttributes.TryAdd(logicalName, result);
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException(
                            $"Unable to retrieve metadata/primaryattribute for entity: {logicalName}. Metadata is: {metabase}");
                    }
                    container.Logger.Log($"Returning {result}");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    if (ex.Message.Contains("Could not find an entity with specified entity name"))
                    {
                        container.Logger.Log("Slim: PrimaryAttribute: Entity not found");
                        PrimaryNameAttributes.TryRemove(logicalName, out result);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    container.Logger.EndSection();
                }
            }
            return new Lazy<string>(() => result);
        }

        /// <summary>
        /// Gets primary name attribute of the entity
        /// </summary>
        /// <returns></returns>
        private Lazy<string> GetPrimaryNameAttribute()
        {
            var result = string.Empty;

            if (PrimaryNameAttributes.ContainsKey(logicalName))
            {
                result = PrimaryNameAttributes[logicalName];
            }
            else
            {
                container.Logger.StartSection($"Getting name of primary attribute on '{logicalName}'");
                try
                {
                    var request = new RetrieveEntityRequest()
                    {
                        LogicalName = logicalName,
                        EntityFilters = EntityFilters.Entity,
                        RetrieveAsIfPublished = true
                    };

                    var response = (RetrieveEntityResponse)container.Service.Execute(request);

                    var metabase = response.EntityMetadata;

                    container.Logger.Log("Metadata retrieved");
                    if (metabase is EntityMetadata meta)
                    {
                        result = meta.PrimaryNameAttribute;
                        PrimaryNameAttributes.TryAdd(logicalName, result);
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException(
                            $"Unable to retrieve metadata/primaryattribute for entity: {logicalName}. Metadata is: {metabase.ToString()}");
                    }
                    container.Logger.Log($"Returning {result}");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    if (ex.Message.Contains("Could not find an entity with specified entity name"))
                    {
                        container.Logger.Log("Slim: PrimaryAttribute: Entity not found");
                        PrimaryNameAttributes.TryRemove(logicalName, out result);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    container.Logger.EndSection();
                }
            }

            return new Lazy<string>(() => result);
        }
    }
}