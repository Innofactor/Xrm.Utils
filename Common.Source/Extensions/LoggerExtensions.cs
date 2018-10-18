namespace Innofactor.Xrm.DevUtils.Common.Extensions
{
    using System;
    using Microsoft.Xrm.Sdk;

    public static class LoggerExtensions
    {
        #region Private Methods

        public static void Trace(this ITracingService logger, Exception ex)
        {
            logger.Trace(ex.ToString());
            logger.Trace(ex.Message);
            logger.Trace(ex.Source);
            logger.Trace(ex.StackTrace);
            logger.Trace("---------------------------------------------------------");
        }

        #endregion Private Methods
    }
}