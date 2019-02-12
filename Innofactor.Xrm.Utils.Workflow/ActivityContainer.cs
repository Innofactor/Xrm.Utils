namespace Innofactor.Xrm.Utils.Workflow
{
    using System;
    using System.Activities;
    using System.Dynamic;
    using Innofactor.Xrm.Utils.Workflow.Extensions;
    using Innofactor.Xrm.Utils.Workflow.Interfaces;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    public class ActivityContainer : IActivityExecutionContainer
    {
        #region Private Fields

        private Lazy<ITracingService> logger;

        private Lazy<IOrganizationService> service;

        #endregion Private Fields

        #region Public Constructors

        public ActivityContainer(CodeActivityContext activityContext)
        {
            ActivityContext = activityContext;
            WorkflowContext = ActivityContext.GetExtension<IWorkflowContext>();

            // Reset values of entities, if they was already used —
            // this move will set fresh values for cached loggers and services
            logger = new Lazy<ITracingService>(() => ActivityContext.GetExtension<ITracingService>());
            service = new Lazy<IOrganizationService>(() => ActivityContext.GetOrganizationService(WorkflowContext.UserId));

            Values = new ExpandoObject();
            Values.IndentationLevel = 0;
        }

        #endregion Public Constructors

        #region Public Properties

        public CodeActivityContext ActivityContext
        {
            get;
        }

        /// Get instance of the <see cref="ILoggable" /> assosiated with current container
        /// </summary>
        public ITracingService Logger =>
            logger.Value;

        public EntityReference PrimaryEntityReference =>
            new EntityReference(WorkflowContext.PrimaryEntityName, WorkflowContext.PrimaryEntityId);

        /// <summary>
        /// Gets instance of <see cref="IServicable" /> assosiated with current container
        /// </summary>
        public IOrganizationService Service =>
            service.Value;

        public dynamic Values
        {
            get;
        }

        public IWorkflowContext WorkflowContext
        {
            get;
        }

        #endregion Public Properties

        #region Public Methods

        public T GetCodeActivityParameter<T>(InArgument<T> parameter) =>
            parameter.Get(ActivityContext);

        public void SetCodeActivityParameter<T>(OutArgument<T> parameter, T value) =>
            parameter.Set(ActivityContext, value);

        #endregion Public Methods
    }
}