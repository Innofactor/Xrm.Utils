namespace Innofactor.Xrm.Utils.Common.Fluent.Access
{
    using Innofactor.Xrm.Utils.Common.Extensions;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;

    public class OperationsSet2 : Information
    {
        internal OperationsSet2(IExecutionContainer container, EntityReference principal, EntityReference target)
            : base(container, principal, target)
        {
        }

        /// <summary>
        /// Assigns current record to given principal
        /// </summary>
        /// <returns></returns>
        public bool Assign()
        {
            try
            {
                container.Service.Execute(new AssignRequest()
                {
                    Assignee = principal,
                    Target = target
                });
                container.Logger.Log($"Assigned {target.LogicalName} to {principal.LogicalName} {principal.Id}");

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Grants access to current record to given principal
        /// </summary>
        /// <param name="accessMask"></param>
        /// <returns></returns>
        public bool Grant(AccessRights accessMask)
        {
            try
            {
                container.Service.Execute(new GrantAccessRequest()
                {
                    PrincipalAccess = new PrincipalAccess()
                    {
                        Principal = principal,
                        AccessMask = accessMask
                    },
                    Target = target
                });

                container.Logger.Log($"{principal.LogicalName}:{principal.Id} was granted {accessMask} to {target.LogicalName}:{target.Id}");

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Revokes access from current record from given principal
        /// </summary>
        /// <returns></returns>
        public bool Revoke()
        {
            try
            {
                container.Service.Execute(new RevokeAccessRequest()
                {
                    Revokee = principal,
                    Target = target
                });

                container.Logger.Log($"{principal.LogicalName}:{principal.Id} was revoked from {target.LogicalName}:{target.Id}");

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}