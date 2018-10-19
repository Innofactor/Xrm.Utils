namespace Innofactor.Xrm.Utils.Common.Fluent.Access
{
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    public class Information : InformationBase
    {
        protected readonly EntityReference principal;
        protected readonly EntityReference target;

        internal Information(IExecutionContainer container, EntityReference principal)
            : base(container) =>
            this.principal = principal;

        internal Information(IExecutionContainer container, EntityReference principal, EntityReference target)
            : this(container, principal) =>
            this.target = target;
    }
}