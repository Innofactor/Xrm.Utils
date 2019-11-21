namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Extension methods for IContainable classes
    /// </summary>
    public static partial class ContainerExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <returns>The Guid of the created entity.</returns>
        public static Entity Create(this IExecutionContainer container, Entity entity)
        {
            entity.Id = container.Service.Create(entity);
            container.Log($"Created {entity.LogicalName}:{entity.Id}");

            return entity;
        }

        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        public static void Delete(this IExecutionContainer container, Entity entity)
        {
            if (entity.Id.Equals(Guid.Empty))
            {
                container.Log("Cannot delete - guid is empty");
                return;
            }

            container.Service.Delete(entity.LogicalName, entity.Id);
            container.Log($"Deleted {entity.LogicalName}:{entity.Id}");
        }

        /// <summary>Encapsulated Retrieve method to be invoked on the service</summary>
        /// <param name="container"></param>
        /// <param name="reference"></param>
        /// <param name="columns">Set of colums with which entity should be reloaded</param>
        /// <returns></returns>
        public static Entity Retrieve(this IExecutionContainer container, EntityReference reference, params string[] columns) =>
            container.Service.Retrieve(reference.LogicalName, reference.Id, new ColumnSet(columns));

        /// <summary>Encapsulated Retrieve method to be invoked on the service</summary>
        /// <param name="container"></param>
        /// <param name="reference"></param>
        /// <param name="columnSet"></param>
        /// <returns></returns>
        public static Entity Retrieve(this IExecutionContainer container, EntityReference reference, ColumnSet columnSet) =>
            container.Service.Retrieve(reference.LogicalName, reference.Id, columnSet);

        /// <summary>Encapsulated Retrieve method to be invoked on the service</summary>
        /// <param name="container"></param>
        /// <param name="entityName"></param>
        /// <param name="id"></param>
        /// <param name="columns">Set of colums with which entity should be reloaded</param>
        /// <returns></returns>
        public static Entity Retrieve(this IExecutionContainer container, string entityName, Guid id, params string[] columns) =>
            container.Service.Retrieve(entityName, id, new ColumnSet(columns));

        /// <summary>Encapsulated Retrieve method to be invoked on the service</summary>
        /// <param name="container"></param>
        /// <param name="entityName"></param>
        /// <param name="id"></param>
        /// <param name="columnSet"></param>
        /// <returns></returns>
        public static Entity Retrieve(this IExecutionContainer container, string entityName, Guid id, ColumnSet columnSet) =>
            container.Service.Retrieve(entityName, id, columnSet);

        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static EntityCollection RetrieveMultiple(this IExecutionContainer container, QueryBase query) =>
            container.Service.RetrieveMultiple(query);

        /// <summary>Retrieve objects matching given criteria</summary>
        /// <param name="container"></param>
        /// <param name="entity">Entity's logical name</param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <param name="columns"></param>
        /// <returns>EntityCollection that encapsulate the resulting Entity records</returns>
        public static EntityCollection RetrieveMultiple(this IExecutionContainer container, string entity, string[] attribute, object[] value, ColumnSet columns)
        {
            var query = new QueryByAttribute(entity);
            query.Attributes.AddRange(attribute);
            query.Values.AddRange(value);
            query.ColumnSet = columns;
            return RetrieveMultiple(container, query);
        }

        /// <summary>
        /// Retrieve All records using QueryExpression
        /// </summary>
        /// <param name="container"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static EntityCollection RetrieveAll(this IExecutionContainer container, QueryExpression query)
        {
            query.PageInfo = new PagingInfo
            {
                Count = 5000,
                PageNumber = 1,
                ReturnTotalRecordCount = false,
                PagingCookie = null
            };

            var results = new EntityCollection();

            while (true)
            {
                var request = GetRequest(query);

                var response = container.Service.Execute(request);

                var batch = ((RetrieveMultipleResponse)((OrganizationResponseCollection)response.Results["Responses"])[0])
                    .EntityCollection;

                results.AddRange(batch);

                if (!batch.MoreRecords)
                {
                    break;
                }

                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = batch.PagingCookie;
            }

            return results;
        }

        private static ExecuteTransactionRequest GetRequest(QueryExpression query)
        {
            var request = new ExecuteTransactionRequest()
            {
                ReturnResponses = true,
                Requests = new OrganizationRequestCollection()
            };

            request.Requests.Add(new RetrieveMultipleRequest { Query = query });

            return request;
        }
        /// <summary>
        /// Save the entity record. If it has a valid Id it will be updated, otherwise new record created.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        public static void Save(this IExecutionContainer container, Entity entity)
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
        /// </summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        public static void Update(this IExecutionContainer container, Entity entity)
        {
            container.Service.Update(entity);
            container.Log($"Updated {entity.LogicalName} {entity.Id} with {entity.Attributes.Count} attributes");
        }
    }
}