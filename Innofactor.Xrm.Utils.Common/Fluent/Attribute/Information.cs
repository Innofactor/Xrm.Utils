namespace Innofactor.Xrm.Utils.Common.Fluent.Attribute
{
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    public class Information : InformationBase
    {
        protected readonly string name;
        protected readonly Entity target;

        internal Information(IExecutionContainer container, string name)
            : base(container) =>
            this.name = name;

        internal Information(IExecutionContainer container, string name, Entity target)
            : this(container, name) =>
            this.target = target;
    }
}