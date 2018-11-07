namespace Innofactor.Xrm.Utils.Workflow.Extensions
{
    using System;
    using System.Activities;
    using Microsoft.Xrm.Sdk;

    public static class CodeActivityContextExtensions
    {
        #region Public Methods

        public static IOrganizationService GetOrganizationService(this CodeActivityContext context, Guid? userId) =>
            context.GetOrganizationServiceFactory().CreateOrganizationService(userId);

        public static IOrganizationServiceFactory GetOrganizationServiceFactory(this CodeActivityContext context) =>
            context.GetExtension<IOrganizationServiceFactory>();

        #endregion Public Methods
    }
}