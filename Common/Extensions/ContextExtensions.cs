namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using System.Linq;
    using Innofactor.Xrm.Utils.Common.Constants;
    using Microsoft.Xrm.Sdk;

    public static class ContextExtensions
    {
        /// <summary>
        /// Retrieves EntityId from the Context
        /// Create, Update, Delete, SetState, Assign, DeliverIncoming
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Guid GetEntityId(this IPluginExecutionContext context)
        {
            switch (context.MessageName)
            {
                case MessageName.Create:
                    if (context.Stage == MessageProcessingStage.BeforeMainOperationOutsideTransaction ||
                        context.Stage == MessageProcessingStage.BeforeMainOperationInsideTransaction)
                    {
                        return Guid.Empty;
                    }
                    else
                    {
                        if (context.OutputParameters.Contains(ParameterName.Id))
                        {
                            return (Guid)context.OutputParameters[ParameterName.Id];
                        }
                    }
                    break;

                case MessageName.DeliverIncoming:
                    if (context.Stage == MessageProcessingStage.BeforeMainOperationOutsideTransaction ||
                        context.Stage == MessageProcessingStage.BeforeMainOperationInsideTransaction)
                    {
                        return Guid.Empty;
                    }
                    else
                    {
                        if (context.OutputParameters.Contains(ParameterName.EmailId))
                        {
                            return (Guid)context.OutputParameters[ParameterName.EmailId];
                        }
                    }
                    break;

                case MessageName.Update:
                case MessageName.Reschedule:
                    if (context.InputParameters[ParameterName.Target] is Entity)
                    {
                        return ((Entity)context.InputParameters[ParameterName.Target]).Id;
                    }
                    break;

                case MessageName.Delete:
                case MessageName.Assign:
                case MessageName.GrantAccess:
                case MessageName.Handle:
                    if (context.InputParameters[ParameterName.Target] is EntityReference)
                    {
                        return ((EntityReference)context.InputParameters[ParameterName.Target]).Id;
                    }
                    break;

                case MessageName.SetState:
                case MessageName.SetStateDynamicEntity:
                    return ((EntityReference)context.InputParameters[ParameterName.EntityMoniker]).Id;

                default:
                    if (context.InputParameters.Contains(ParameterName.Target) &&
                        (context.InputParameters[ParameterName.Target] is EntityReference))
                    {
                        return ((EntityReference)context.InputParameters[ParameterName.Target]).Id;
                    }

                    //Try by best route else fail
                    return Guid.Empty;
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Returnerar true om något attribut i listan återfinns i Target i Context.
        /// Om det är ett Create-message så returneras alltid true
        /// Om Target saknas så returneras alltid true
        /// </summary>
        /// <param name="context"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static bool IsChangedAnyOf(this IPluginExecutionContext context, params string[] attributes)
        {
            if (context.InputParameters.Contains(ParameterName.Target) && context.InputParameters[ParameterName.Target] is Entity)
            {
                return ((Entity)context.InputParameters[ParameterName.Target]).Attributes.Keys.Intersect(attributes).Any();
            }
            else
            {
                return false;
            }
        }
    }
}