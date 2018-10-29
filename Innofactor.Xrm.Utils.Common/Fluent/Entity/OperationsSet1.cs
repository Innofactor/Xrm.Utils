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
        #region Private Fields

        private readonly Entity target;

        #endregion Private Fields

        #region Internal Constructors

        internal OperationsSet1(IExecutionContainer container, Entity target)
            : base(container) =>
            this.target = target;

        #endregion Internal Constructors

        #region Private Properties

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

        #endregion Private Properties

        #region Public Methods

        public override string ToString()
        {
            if (target == null)
            {
                return string.Empty;
            }

            var result = new StringBuilder();

            if (!string.IsNullOrEmpty(target.LogicalName))
            {
                // Adding entity logical name
                result.Append(target.LogicalName);
            }

            if (!target.Id.Equals(Guid.Empty))
            {
                if (result.Length > 0)
                {
                    result.Append(":");
                }

                result.Append(target.Id.ToString());
            }

            foreach (var key in "name;fullname;title;subject".Split(';'))
            {
                if (target.Contains(key))
                {
                    if (result.Length > 0)
                    {
                        result.Append(" ");
                    }

                    result.Append("(");
                    result.Append(target.Attributes[key] as string);
                    result.Append(")");

                    break;
                }
            }

            return result.ToString();
        }

        public OperationsSet5 Via(string intersectionName) =>
            new OperationsSet5(container, target, intersectionName);

        #endregion Public Methods
    }
}