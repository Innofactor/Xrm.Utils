namespace Innofactor.Xrm.Utils.Common.Fluent.Attribute
{
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    public class OperationsSet1 : Information
    {
        internal OperationsSet1(IContainable container, string name)
            : base(container, name)
        {
        }

        /// <summary>
        /// Adds information about entity to work with
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public OperationsSet2 On(Entity target) =>
            new OperationsSet2(container, name, target);
    }
}