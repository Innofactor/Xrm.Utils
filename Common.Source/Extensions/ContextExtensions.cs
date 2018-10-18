namespace Innofactor.Xrm.DevUtils.Common.Extensions
{
    using System;
    using Innofactor.Xrm.DevUtils.Common.Constants;
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
    }
}