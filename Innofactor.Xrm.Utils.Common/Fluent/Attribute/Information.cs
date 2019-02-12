namespace Innofactor.Xrm.Utils.Common.Fluent.Attribute
{
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    public class Information : InformationBase
    {
        #region Protected Fields

        protected readonly string name;
        protected readonly Entity target;

        #endregion Protected Fields

        #region Internal Constructors

        internal Information(IExecutionContainer container, string name)
            : base(container)
        {
            this.name = name;
        }

        internal Information(IExecutionContainer container, string name, Entity target)
            : this(container, name)
        {
            this.target = target;
        }

        #endregion Internal Constructors
    }
}