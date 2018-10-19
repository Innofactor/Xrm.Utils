namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    public static partial class ContainerExtensions
    {
        /// <summary>
        /// Initiates work with attributes
        /// </summary>
        /// <param name="container"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Fluent.Attribute.OperationsSet1 Attribute(this IExecutionContainer container, string name) =>
            new Fluent.Attribute.OperationsSet1(container, name);

        /// <summary>
        /// Initiates work with entitites
        /// </summary>
        /// <param name="container"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Fluent.Entity.OperationsSet1 Entity(this IExecutionContainer container, Entity target) =>
            new Fluent.Entity.OperationsSet1(container, target);

        /// <summary>
        /// Initiates work with entitites
        /// </summary>
        /// <param name="container"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Fluent.Entity.OperationsSet1 Entity(this IExecutionContainer container, EntityReference target) =>
            new Fluent.Entity.OperationsSet1(container, new Entity(target.LogicalName, target.Id));

        /// <summary>
        /// Initiates work with entitites
        /// </summary>
        /// <param name="container"></param>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        public static Fluent.Entity.OperationsSet2 Entity(this IExecutionContainer container, string logicalName) =>
            new Fluent.Entity.OperationsSet2(container, logicalName);

        /// <summary>
        /// Initiates work with principals which is followed with access related opetations: Grant / Revoke / Assign
        /// </summary>
        /// <param name="container"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static Fluent.Access.OperationsSet1 Principal(this IExecutionContainer container, Entity principal) =>
            container.Principal(principal.ToEntityReference());

        /// <summary>
        /// Initiates work with principals which is followed with access related opetations: Grant / Revoke / Assign
        /// </summary>
        /// <param name="container"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static Fluent.Access.OperationsSet1 Principal(this IExecutionContainer container, EntityReference principal) =>
            new Fluent.Access.OperationsSet1(container, principal);
    }
}