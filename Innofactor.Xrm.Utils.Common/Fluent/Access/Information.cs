namespace Innofactor.Xrm.Utils.Common.Fluent.Access
{
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    public class Information : InformationBase
    {
        #region Protected Fields

        protected readonly EntityReference principal;
        protected readonly EntityReference target;

        #endregion Protected Fields

        #region Internal Constructors

        internal Information(IContainable container, EntityReference principal)
            : base(container) =>
            this.principal = principal;

        internal Information(IContainable container, EntityReference principal, EntityReference target)
            : this(container, principal) =>
            this.target = target;

        #endregion Internal Constructors
    }
}