namespace Innofactor.Xrm.Utils.Workflow.Interfaces
{
    using System.Activities;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Plugin specific extensions to IContainable
    /// </summary>
    public interface IActivityContainer : IExecutionContainer
    {
        #region Public Properties

        /// <summary>
        /// Gets instance of the <see cref="CodeActivityContext" /> assosiated with current container
        /// </summary>
        CodeActivityContext ActivityContext
        {
            get;
        }

        /// <summary>
        /// Gets instance of the <see cref="IWorkflowContext" /> assosiated with current container
        /// </summary>
        IWorkflowContext WorkflowContext
        {
            get;
        }

        #endregion Public Properties
    }
}