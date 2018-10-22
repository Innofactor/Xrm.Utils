namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Caching;
    using System.Text;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Metadata;

    public static partial class ContainerExtensions
    {
        private static readonly MemoryCache cache = new MemoryCache("metadata");

        /// <summary>
        /// Method to be used when retrieving metadata, to be able to benefit from metadata caching abilities
        /// </summary>
        /// <param name="container"></param>
        /// <param name="request">Metadata request</param>
        /// <returns></returns>
        public static MetadataBase Execute(this IExecutionContainer container, RetrieveEntityRequest request)
        {
            var key = request.LogicalName;
            var value = default(MetadataBase);

            if (cache.Contains(key))
            {
                value = cache.Get(key) as MetadataBase;
            }
            else
            {
                value = ((RetrieveEntityResponse)container.Service.Execute(request)).EntityMetadata;
                cache.Add(new CacheItem(key, value), new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromMinutes(5) });
            }

            return value;
        }

        /// <summary>
        /// Method to be used when retrieving metadata, to be able to benefit from metadata caching abilities
        /// </summary>
        /// <param name="container"></param>
        /// <param name="request">Metadata request</param>
        /// <returns></returns>
        public static MetadataBase Execute(this IExecutionContainer container, RetrieveAttributeRequest request)
        {
            var key = $"{request.EntityLogicalName} @ {request.LogicalName}";
            var value = default(MetadataBase);

            if (cache.Contains(key))
            {
                value = cache.Get(key) as MetadataBase;
            }
            else
            {
                value = ((RetrieveAttributeResponse)container.Service.Execute(request)).AttributeMetadata;
                cache.Add(new CacheItem(key, value), new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromMinutes(5) });
            }

            return value;
        }
    }
}
