namespace Innofactor.Xrm.Utils.Workflow
{
    using System;
    using System.Activities;
    using System.Dynamic;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Innofactor.Xrm.Utils.Common.Loggers;
    using Innofactor.Xrm.Utils.Workflow.Extensions;
    using Innofactor.Xrm.Utils.Workflow.Interfaces;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// 
    /// </summary>
    public class ActivityContainer : IActivityExecutionContainer
    {
        #region Private Fields

        private Lazy<ILoggable> logger;

        private Lazy<IOrganizationService> service;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityContext"></param>
        public ActivityContainer(CodeActivityContext activityContext)
        {
            ActivityContext = activityContext;
            WorkflowContext = ActivityContext.GetExtension<IWorkflowContext>();

            // Reset values of entities, if they was already used —
            // this move will set fresh values for cached loggers and services
            logger = new Lazy<ILoggable>(() => new CRMLogger(ActivityContext.GetExtension<ITracingService>()));
            service = new Lazy<IOrganizationService>(() => ActivityContext.GetOrganizationService(WorkflowContext.UserId));

            Values = new ExpandoObject();
            Values.IndentationLevel = 0;
        }

        #endregion Public Constructors

        #region Public Properties
        /// <summary>
        /// 
        /// </summary>
        public CodeActivityContext ActivityContext
        {
            get;
        }

        /// <summary>
        /// Get instance of the <see cref="ILoggable" /> assosiated with current container
        /// </summary>
        public ILoggable Logger =>
            logger.Value;

        /// <summary>
        /// 
        /// </summary>
        public EntityReference PrimaryEntityReference =>
            new EntityReference(WorkflowContext.PrimaryEntityName, WorkflowContext.PrimaryEntityId);

        /// <summary>
        /// Gets instance of <see cref="IOrganizationService" /> assosiated with current container
        /// </summary>
        public IOrganizationService Service =>
            service.Value;

        /// <summary>
        /// 
        /// </summary>
        public dynamic Values
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public IWorkflowContext WorkflowContext
        {
            get;
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public T GetCodeActivityParameter<T>(InArgument<T> parameter) =>
            parameter.Get(ActivityContext);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        public void SetCodeActivityParameter<T>(OutArgument<T> parameter, T value) =>
            parameter.Set(ActivityContext, value);

        #endregion Public Methods
    }
}