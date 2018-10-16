namespace Innofactor.Xrm.DevUtils.Common.Interfaces
{
    /// <summary>
    /// Core object that helps to keep all objects and methods needed for CRM development in
    /// package easy to access and operate
    /// </summary>
    public interface IContainable
    {
        /// <summary>
        /// Get instance of the <see cref="ILoggable"/> assosiated with current container
        /// </summary>
        ILoggable Logger
        {
            get;
        }

        /// <summary>
        /// Gets instance of <see cref="IServicable"/> assosiated with current container
        /// </summary>
        IServicable Service
        {
            get;
        }
    }
}