namespace Innofactor.Xrm.Utils.Common.Fluent.Attribute
{
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    public class OperationsSet1 : Information
    {
        #region Internal Constructors

        internal OperationsSet1(IExecutionContainer container, string name)
            : base(container, name)
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
            new OperationsSet2(container, name, target);

        #endregion Public Methods
    }
}