namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using Microsoft.Xrm.Sdk;

    public static class ServiceProviderExtensions
    {
        #region Public Methods

        public static T Get<T>(this IServiceProvider provider) =>
            (T)provider.GetService(typeof(T));

        public static IOrganizationService GetOrganizationService(this IServiceProvider provider, Guid? userId) =>
            provider.GetOrganizationServiceFactory().CreateOrganizationService(userId);

        public static IOrganizationService GetOrganizationService(this IServiceProvider provider) =>
            provider.GetOrganizationServiceFactory().CreateOrganizationService(provider.GetPluginExecutionContext().UserId);

        public static IOrganizationServiceFactory GetOrganizationServiceFactory(this IServiceProvider provider) =>
            (IOrganizationServiceFactory)provider.GetService(typeof(IOrganizationServiceFactory));

        public static IPluginExecutionContext GetPluginExecutionContext(this IServiceProvider provider) =>
                    (IPluginExecutionContext)provider.GetService(typeof(IPluginExecutionContext));

        public static ITracingService GetTracingService(this IServiceProvider provider) =>
            (ITracingService)provider.GetService(typeof(ITracingService));

        #endregion Public Methods
    }
}