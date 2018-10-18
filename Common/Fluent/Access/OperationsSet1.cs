namespace Innofactor.Xrm.Utils.Common.Fluent.Access
{
    using Cinteros.Crm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    public class OperationsSet1 : Information
    {
        #region Internal Constructors

        internal OperationsSet1(IContainable container, EntityReference principal)
            : base(container, principal)
        {
        }

        #endregion Internal Constructors

        #region Public Methods

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

        #endregion Public Methods
    }
}