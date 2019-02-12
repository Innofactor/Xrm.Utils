namespace Innofactor.Xrm.Utils.Common.Fluent
{
    using System.ComponentModel;
    using Innofactor.Xrm.Utils.Common.Interfaces;

    public class InformationBase
    {
        protected readonly IExecutionContainer container;

        internal InformationBase(IExecutionContainer container)
        {
            this.container = container;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) =>
            base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() =>
            base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() =>
            base.ToString();
    }
}