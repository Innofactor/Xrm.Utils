namespace Innofactor.Xrm.Utils.Common.Fluent
{
    using System.ComponentModel;
    using Innofactor.Xrm.Utils.Common.Interfaces;

    public class InformationBase
    {
        #region Protected Fields

        protected readonly IExecutionContainer container;

        #endregion Protected Fields

        #region Internal Constructors

        internal InformationBase(IExecutionContainer container)
            => this.container = container;

        #endregion Internal Constructors

        #region Public Methods

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) =>
            base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() =>
            base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() =>
            base.ToString();

        #endregion Public Methods
    }
}