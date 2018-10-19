namespace Innofactor.Xrm.Utils.Common.Fluent.Access
{
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    public class Information : InformationBase
    {
        protected readonly EntityReference principal;
        protected readonly EntityReference target;

        internal Information(IContainable container, EntityReference principal)
            : base(container) =>
            this.principal = principal;

        internal Information(IContainable container, EntityReference principal, EntityReference target)
            : this(container, principal) =>
            this.target = target;
    }
}