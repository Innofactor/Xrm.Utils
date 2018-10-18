namespace Innofactor.Xrm.DevUtils.Common.Interfaces
{
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Core object that helps to keep all objects and methods needed for CRM development in
    /// package easy to access and operate
    /// </summary>
    public interface IContainable
    {
        #region Public Properties

        /// <summary>
        /// Get instance of the <see cref="ITracingService"/> assosiated with current container
        /// </summary>
        ITracingService Logger
        {
            get;
        }

        /// <summary>
        /// Gets instance of <see cref="IOrganizationService"/> assosiated with current container
        /// </summary>
        IOrganizationService Service
        {
            get;
        }

        #endregion Public Properties
    }
}