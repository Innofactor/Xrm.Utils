namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Extension methods for IContainable classes
    /// </summary>
    public static partial class ContainerExtensions
    {
        #region Public Methods

        /// <summary>Associates current record with relatedentity, using specified intersect relationship</summary>
        /// <param name="container"></param>
        /// <param name="entity">Current entity</param>
        /// <param name="relatedentity">Related entity</param>
        /// <param name="intersect">Name of the intersect relationship/entity</param>
        /// <remarks>To be used with N:N-relationships.</remarks>
        /// <exception cref="FaultException{TDetail}">
        /// <strong>TDetail</strong> may be typed as:
        /// <para>
        /// <see cref="OrganizationServiceFault" />: Thrown when association already exists.
        /// </para>
        /// </exception>
        public static void Associate(this IExecutionContainer container, Entity entity, Entity relatedentity, string intersect)
        {
            var collection = new EntityCollection();
            collection.Add(relatedentity);
            container.Associate(entity, collection, intersect);
        }

        /// <summary>Associates current record with relatedentities, using specified intersect relationship</summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="relatedEntities">Collection of the entities to be related to current entity</param>
        /// <param name="intersect">Name of the intersect relationship/entity</param>
        /// <remarks>To be used with N:N-relationships.</remarks>
        /// <exception cref="FaultException{TDetail}">
        /// <strong>TDetail</strong> may be typed as:
        /// <para>
        /// <see cref="OrganizationServiceFault" />: Thrown when any of the associations already exists.
        /// </para>
        /// </exception>
        public static void Associate(this IExecutionContainer container, Entity entity, EntityCollection relatedEntities, string intersect)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            container.Associate(entity, relatedEntities, intersect, int.MaxValue);
        }

        /// <summary>Associates current record with relatedentities, using specified intersect relationship</summary>
        /// <param name="entity"></param>
        /// <param name="container"></param>
        /// <param name="relatedEntities">Collection of the entities to be related to current entity</param>
        /// <param name="intersect">Name of the intersect relationship/entity</param>
        /// <param name="batchSize">Optional. Determines the max number of entities to associate per request</param>
        /// <remarks>To be used with N:N-relationships.</remarks>
        /// <exception cref="FaultException{TDetail}">
        /// <strong>TDetail</strong> may be typed as:
        /// <para>
        /// <see cref="OrganizationServiceFault" />: Thrown when any of the associations already exists.
        /// </para>
        /// </exception>
        public static void Associate(this IExecutionContainer container, Entity entity, EntityCollection relatedEntities, string intersect, int batchSize)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var role = default(EntityRole?);
            if (relatedEntities.Entities.Count > 0 && relatedEntities[0].LogicalName == entity.LogicalName)
            {
                // N:N-relation to the same entity, so role have to be specified.
                role = EntityRole.Referencing;
            }

            if (batchSize < 1)
            {
                throw new ArgumentException("batchSize must be larger than zero.");
            }

            var entRefCollection = relatedEntities.ToEntityReferenceCollection();
            var processed = 0;
            while (processed < relatedEntities.Entities.Count)
            {
                var batch = new EntityReferenceCollection(entRefCollection.Skip(processed).Take(batchSize).ToList());
                processed += batch.Count();

                var req = new AssociateRequest
                {
                    Target = entity.ToEntityReference(),
                    Relationship = new Relationship(intersect)
                    {
                        PrimaryEntityRole = role
                    },
                    RelatedEntities = batch
                };
                container.Service.Execute(req);
                container.Log("Associated {0} {1} with {2}", batch.Count, relatedEntities.Entities.Count > 0 ? relatedEntities[0].LogicalName : "", entity.LogicalName);
            }
        }

        /// <summary>
        /// Checks if a property exists in the encapsulated Entity
        /// </summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="attribute">Name of property to check</param>
        /// <returns></returns>
        public static Entity Ensure(this IExecutionContainer container, Entity entity, string attribute) =>
            entity.Contains(attribute)
            ? entity
            : container.Reload(entity, attribute);


        /// <summary>
        /// Converts QueryExpression to FetchXml
        /// </summary>
        /// <param name="container"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string Convert(this IExecutionContainer container, QueryExpression query)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");

            try
            {
                var request = new QueryExpressionToFetchXmlRequest()
                {
                    Query = query
                };

                var response = (QueryExpressionToFetchXmlResponse)container?.Service?.Execute(request);

                if (response != null)
                {
                    container.Log("Query was converted successfully.");
                }
                else
                {
                    container.Log("It was an issue converting query.");
                }

                return response?.FetchXml;
            }
            finally
            {
                container.EndSection();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="entity1"></param>
        /// <param name="entity2"></param>
        /// <returns></returns>
        public static Entity Merge(this IExecutionContainer container, Entity entity1, Entity entity2)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");

            container.Log($"Merging {entity1.LogicalName} {container.Entity(entity1).ToString()} with {entity2.LogicalName} {container.Entity(entity2).ToString()}");

            var merge = entity1.CloneAttributes();
            foreach (var prop in entity2.Attributes)
            {
                if (!merge.Attributes.Contains(prop.Key))
                {
                    merge.Attributes.Add(prop);
                }
            }

            container.Log($"Base entity had {entity1.Attributes.Count} attributes. Second entity {entity2.Attributes.Count}. Merged entity has {merge.Attributes.Count}");
            container.EndSection();
            return merge;
        }

        /// <summary>Reloads encapsulated Entity from database</summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <remarks>ToStringWithEntityName() is replaced with entity.LogicalName</remarks>
        public static Entity Reload(this IExecutionContainer container, Entity entity) =>
            container.Reload(entity, new ColumnSet());

        /// <summary>Reloads encapsulated Entity from database</summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="columns">Set of colums with which entity should be reloaded</param>
        /// <remarks>ToStringWithEntityName() is replaced with entity.LogicalName</remarks>
        public static Entity Reload(this IExecutionContainer container, Entity entity, params string[] columns) =>
            container.Reload(entity, new ColumnSet(columns));

        /// <summary>Reloads encapsulated Entity from database</summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="columns">Set of colums with which entity should be reloaded</param>
        /// <remarks>ToStringWithEntityName() is replaced with entity.LogicalName</remarks>
        public static Entity Reload(this IExecutionContainer container, Entity entity, ColumnSet columns)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");

            container.StartSection($"Reloading {container.Entity(entity).ToString()}.");

            foreach (var attr in entity.Attributes.Keys)
            {
                if (attr.Contains('.'))
                {
                    throw new InvalidPluginExecutionException($"Cannot reload entity {entity.LogicalName} with aliased attributes ({attr})");
                }
            }

            if (columns == null)
            {
                columns = new ColumnSet();
            }

            if (!columns.Columns.Any() && !columns.AllColumns)
            {
                foreach (var attr in entity.Attributes.Keys)
                {
                    columns.AddColumn(attr);
                }
            }

            entity = container.Retrieve(entity.ToEntityReference(), columns);
            container.EndSection();

            return entity;
        }

        /// <summary>Update state and status of current record</summary>
        /// <remarks>
        /// http://msdynamicscrmblog.wordpress.com/2013/10/26/status-and-status-reason-values-in-dynamics-crm-2013/comment-page-1/
        /// ToStringWithEntityName() is replaced with entity.LogicalName
        /// </remarks>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="state">Active=0 and Inactive=1</param>
        /// <param name="status">Active=1 and Inactive=2</param>
        public static SetStateResponse SetState(this IExecutionContainer container, Entity entity, int state, int status)
        {
            container.Log($"Setting state {state} {status} on {entity.LogicalName}");

            var response = container.Service.Execute(new SetStateRequest()
            {
                EntityMoniker = entity.ToEntityReference(),
                State = new OptionSetValue(state),
                Status = new OptionSetValue(status)
            }) as SetStateResponse;

            container.Log("SetState completed");

            return response;
        }

        #endregion Public Methods
    }
}