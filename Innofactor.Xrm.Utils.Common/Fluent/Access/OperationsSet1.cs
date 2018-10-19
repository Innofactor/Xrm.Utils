namespace Innofactor.Xrm.Utils.Common.Fluent.Access
{
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    public class OperationsSet1 : Information
    {
        internal OperationsSet1(IContainable container, EntityReference principal)
            : base(container, principal)
        {
        }

        /// <summary>
        /// Adds information about entity to work with
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public OperationsSet2 On(Entity target) =>
            On(target.ToEntityReference());

        /// <summary>
        /// Adds information about entity to work with
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public OperationsSet2 On(EntityReference target) =>
            new OperationsSet2(container, principal, target);
    }
}