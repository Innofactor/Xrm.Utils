namespace Innofactor.Xrm.Utils.Common.Fluent.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Innofactor.Xrm.Utils.Common.Extensions;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    public class OperationsSet1 : InformationBase
    {
        private readonly Entity target;

        internal OperationsSet1(IContainable container, Entity target)
            : base(container) =>
            this.target = target;

        private IEnumerable<string> Prefixes
        {
            get
            {
                var entityName = target.LogicalName;

                var result = new List<string>();
                var prefix = new StringBuilder();

                while (entityName.Contains("_"))
                {
                    prefix.Append(entityName.Split('_')[0] + "_");
                    entityName = entityName.Substring(entityName.IndexOf('_') + 1);
                    result.Add(prefix.ToString());
                }

                return result;
            }
        }

        public override string ToString()
        {
            if (target == null)
            {
                return string.Empty;
            }
            foreach (var commonattribute in "name;fullname;title;subject".Split(';'))
            {
                if (target.Contains(commonattribute))
                {
                    return container.Attribute(commonattribute).On(target).ToString();
                }
            }
            foreach (var prefix in Prefixes)
            {
                if (target.Contains(prefix + "name"))
                {
                    return container.Attribute(prefix + "name").On(target).ToString();
                }
            }
            if (target.Id.Equals(Guid.Empty))
            {
                return string.Empty;
            }
            else
            {
                return target.Id.ToString();
            }
        }

        public OperationsSet5 Via(string intersectionName) =>
            new OperationsSet5(container, target, intersectionName);
    }
}