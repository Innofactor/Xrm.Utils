namespace Innofactor.Xrm.Utils.Workflow
{
    using System;
    using System.Activities;
    using Innofactor.Xrm.Utils.Workflow.Interfaces;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    public class ActivityContainer : IActivityContainer
    {
        private Lazy<IWorkflowContext> workflowContext;
        private Lazy<ITracingService> logger;
        private Lazy<IOrganizationService> service;

        public ActivityContainer(CodeActivityContext activityContext)
        {
            ActivityContext = activityContext;
            workflowContext = new Lazy<IWorkflowContext>(() => activityContext.GetExtension<IWorkflowContext>());

            // Reset values of entities, if they was already used —
            // this move will set fresh values for cached loggers and services
            logger = new Lazy<ITracingService>(() => activityContext.GetExtension<ITracingService>());
            service = new Lazy<IOrganizationService>(() => activityContext.GetExtension<IOrganizationService>());
        }

        public EntityReference PrimaryEntityReference =>
            new EntityReference(WorkflowContext.PrimaryEntityName, WorkflowContext.PrimaryEntityId);

        public CodeActivityContext ActivityContext
        {
            get;
        }

        public IWorkflowContext WorkflowContext =>
            workflowContext.Value;

        /// Get instance of the <see cref="ILoggable" /> assosiated with current container
        /// </summary>
        public ITracingService Logger =>
            logger.Value;

        /// <summary>
        /// Gets instance of <see cref="IServicable" /> assosiated with current container
        /// </summary>
        public IOrganizationService Service =>
            service.Value;

        public T GetCodeActivityParameter<T>(InArgument<T> parameter) =>
            parameter.Get(ActivityContext);

        public void SetCodeActivityParameter<T>(OutArgument<T> parameter, T value) =>
            parameter.Set(ActivityContext, value);
    }
}