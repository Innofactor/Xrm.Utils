namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Extension methods for IContainable classes
    /// </summary>
    public static partial class ContainerExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <returns>The Guid of the created entity.</returns>
        public static Entity Create(this IContainable container, Entity entity)
        {
            entity.Id = container.Service.Create(entity);
            container.Logger.Log($"Created {entity.LogicalName}:{entity.Id}");

            return entity;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        public static void Delete(this IContainable container, Entity entity)
        {
            if (entity.Id.Equals(Guid.Empty))
            {
                container.Logger.Log("Cannot delete - guid is empty");
                return;
            }
            container.Service.Delete(entity.LogicalName, entity.Id);
            container.Logger.Log($"Deleted {entity.LogicalName}:{entity.Id}");
        }

        /// <summary>Encapsulated Retrieve method to be invoked on the service</summary>
        /// <param name="container"></param>
        /// <param name="reference"></param>
        /// <param name="allColumns"></param>
        /// <returns></returns>
        public static Entity Retrieve(this IContainable container, EntityReference reference, bool allColumns) =>
            container.Service.Retrieve(reference.LogicalName, reference.Id, new ColumnSet(allColumns));

        /// <summary>Encapsulated Retrieve method to be invoked on the service</summary>
        /// <param name="container"></param>
        /// <param name="reference"></param>
        /// <param name="columns">Set of colums with which entity should be reloaded</param>
        /// <returns></returns>
        public static Entity Retrieve(this IContainable container, EntityReference reference, params string[] columns) =>
            container.Service.Retrieve(reference.LogicalName, reference.Id, new ColumnSet(columns));

        /// <summary>Encapsulated Retrieve method to be invoked on the service</summary>
        /// <param name="container"></param>
        /// <param name="reference"></param>
        /// <param name="columnSet"></param>
        /// <returns></returns>
        public static Entity Retrieve(this IContainable container, EntityReference reference, ColumnSet columnSet) =>
            container.Service.Retrieve(reference.LogicalName, reference.Id, columnSet);

        /// <summary>Encapsulated Retrieve method to be invoked on the service</summary>
        /// <param name="container"></param>
        /// <param name="entityName"></param>
        /// <param name="id"></param>
        /// <param name="allColumns"></param>
        /// <returns></returns>
        public static Entity Retrieve(this IContainable container, string entityName, Guid id, bool allColumns) =>
            container.Service.Retrieve(entityName, id, new ColumnSet(allColumns));

        /// <summary>Encapsulated Retrieve method to be invoked on the service</summary>
        /// <param name="container"></param>
        /// <param name="entityName"></param>
        /// <param name="id"></param>
        /// <param name="columns">Set of colums with which entity should be reloaded</param>
        /// <returns></returns>
        public static Entity Retrieve(this IContainable container, string entityName, Guid id, params string[] columns) =>
            container.Service.Retrieve(entityName, id, new ColumnSet(columns));

        /// <summary>Encapsulated Retrieve method to be invoked on the service</summary>
        /// <param name="container"></param>
        /// <param name="entityName"></param>
        /// <param name="id"></param>
        /// <param name="columnSet"></param>
        /// <returns></returns>
        public static Entity Retrieve(this IContainable container, string entityName, Guid id, ColumnSet columnSet) =>
            container.Service.Retrieve(entityName, id, columnSet);

        /// <summary>
        ///
        /// </summary>
        /// <param name="container"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static EntityCollection RetrieveMultiple(this IContainable container, QueryBase query) =>
            container.Service.RetrieveMultiple(query);

        /// <summary>Retrieve objects matching given criteria</summary>
        /// <param name="container"></param>
        /// <param name="entity">Entity's logical name</param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <param name="columns"></param>
        /// <returns>EntityCollection that encapsulate the resulting Entity records</returns>
        public static EntityCollection RetrieveMultiple(this IContainable container, string entity, string[] attribute, object[] value, ColumnSet columns)
        {
            var query = new QueryByAttribute(entity);
            query.Attributes.AddRange(attribute);
            query.Values.AddRange(value);
            query.ColumnSet = columns;
            return RetrieveMultiple(container, query);
        }

        /// <summary>
        /// Save the entity record. If it has a valid Id it will be updated, otherwise new record created.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        public static void Save(this IContainable container, Entity entity)
        {
            if (entity.Id.Equals(Guid.Empty))
            {
                container.Create(entity);
            }
            else
            {
                container.Update(entity);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        public static void Update(this IContainable container, Entity entity)
        {
            container.Service.Update(entity);
            container.Logger.Log($"Updated {entity.LogicalName} {entity.Id} with {entity.Attributes.Count} attributes");
        }
    }
}