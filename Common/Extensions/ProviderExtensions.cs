namespace Innofactor.Xrm.DevUtils.Common.Extensions
{
    using System;
    using Microsoft.Xrm.Sdk;

    public static class ProviderExtensions
    {
        public static T Get<T>(this IServiceProvider provider) =>
            (T)provider.GetService(typeof(T));

        public static IPluginExecutionContext GetPluginExecutionContext(this IServiceProvider provider) =>
            (IPluginExecutionContext)provider.GetService(typeof(IPluginExecutionContext));

        public static IOrganizationService GetOrganizationService(this IServiceProvider provider) =>
            (IOrganizationService)provider.GetService(typeof(IOrganizationService));

        public static ITracingService GetTracingService(this IServiceProvider provider) =>
            (ITracingService)provider.GetService(typeof(ITracingService));
    }
}