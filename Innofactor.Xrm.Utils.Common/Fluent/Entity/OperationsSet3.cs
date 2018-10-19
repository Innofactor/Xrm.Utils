namespace Innofactor.Xrm.Utils.Common.Fluent.Entity
{
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    public class OperationsSet3 : InformationBase
    {
        protected readonly EntityReference target;

        protected readonly string logicalName;

        internal OperationsSet3(IExecutionContainer container, string logicalName, EntityReference target)
            : base(container)
        {
            this.logicalName = logicalName;
            this.target = target;
        }

        /// <summary>
        /// Sets <paramref name="name"/> of the attribute or relation via which association was perfomed
        /// </summary>
        /// <param name="name">Name of the attribute or relation</param>
        /// <returns></returns>
        public OperationsSet4 Via(string name) =>
            new OperationsSet4(container, logicalName, target, name);
    }
}