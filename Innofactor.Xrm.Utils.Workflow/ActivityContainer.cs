namespace Innofactor.Xrm.Utils.Workflow
{
    using System;
    using System.Activities;
    using System.ServiceModel;
    using Innofactor.Xrm.Utils.Workflow.Interfaces;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Sdk.Workflow;

    public class ActivityContainer : IActivityContainer
    {
        public ActivityContainer(CodeActivityContext context)
        {
            ActivityContext = context;
            workflowContext = new Lazy<IWorkflowContext>(() => context.GetExtension<IWorkflowContext>());

            // Reset values of entities, if they was already used —
            // this move will set fresh values for cached loggers and services
            logger = new Lazy<ITracingService>(() => provider.Get<ITracingService>());
            service = new Lazy<IOrganizationService>(() => provider.Get<IOrganizationService>());
        }

        public new Action<ActivityContainer> Action { get; set; }

        public CodeActivityContext ActivityContext { get; }

        private Lazy<IWorkflowContext> workflowContext;

        public EntityReference TargetReference => 
            new EntityReference(WorkflowContext.PrimaryEntityName, WorkflowContext.PrimaryEntityId);

        public IWorkflowContext WorkflowContext =>
            workflowContext.Value;

        public void Execute()
        {
            try
            {
                if (Action == null)
                {
                    throw new InvalidPluginExecutionException(string.Format("Main action is not set for {0}", Name));
                }
                Init();
                Action.Invoke(this);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                var log = Logger;
                if (log == null)
                {
                    log = new WorkflowLogger(ActivityContext, LogName, true);
                }
                log.Log(ex);
                log.CloseLog();
                throw new InvalidPluginExecutionException(string.Format("An error occurred in plug-in {0}. {1}: {2}", Name, ex, ex.Detail.Message));
            }
            catch (Exception ex)
            {
                var log = Logger;
                if (log == null)
                {
                    log = new WorkflowLogger(ActivityContext, LogName, true);
                }
                log.Log(ex);
                log.CloseLog();
                throw;
            }
            finally
            {
                if (Logger != null)
                {
                    Logger.CloseLog("Exit");
                }
            }
        }

        public T GetCodeActivityParameter<T>(InArgument<T> parameter)
        {
            var result = parameter.Get(ActivityContext);
            return result;
        }

        public void SetCodeActivityParameter<T>(OutArgument<T> parameter, T value) => parameter.Set(ActivityContext, value);

        protected override ILoggable GetContextLogger()
        {
            if (WorkflowContext != null)
            {
                return new WorkflowLogger(ActivityContext, this.LogName, true);
            }
            return null;
        }

        protected override CrmServiceProxy GetServiceFromContext()
        {
            CrmServiceProxy svc = null;
            if (ActivityContext != null)
            {
                if (Logger != null)
                {
                    Logger.StartSection("CintActivityContainer Service initialization");
                }
                svc = new CrmServiceProxy(ActivityUtils.GetOrganizationService(ActivityContext, Logger))
                {
                    CacheMode = CacheMode.Single,
                };
                if (Logger != null)
                {
                    Logger.EndSection();
                }
            }
            return svc;
        }
    }
}