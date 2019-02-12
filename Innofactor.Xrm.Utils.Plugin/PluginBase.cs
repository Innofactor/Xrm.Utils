namespace Innofactor.Xrm.Utils.Common
{
    using System;
    using Innofactor.Xrm.Utils.Common.Extensions;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// MS CRM Base Plugin. Makes usage of <see cref="PluginContainer" /> more easy.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBase" /> class. Will use refrected name of the calling plugin.
        /// </summary>
        public PluginBase()
        {
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Default entry point for CRM plugin. Validater will be executed. In case of success, log will
        /// be initialized and main code will be invoked
        /// </summary>
        public void Execute(IServiceProvider provider)
        {
            var container = new PluginContainer(provider);

            try
            {
                if (!new Predicate<IPluginExecutionContext>(Validate).Invoke(container.Context))
                {
                    return;
                }

                new Action<IPluginExecutionContainer>(Execute).Invoke(container);
            }
            catch (Exception ex)
            {
                container.Log(ex);
                throw;
            }
        }

        /// <summary>
        /// Main entry point for the plugin
        /// </summary>
        /// <param name="container"></param>
        public abstract void Execute(IPluginExecutionContainer container);

        /// <summary>
        /// Method to validate plugin's execution context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract bool Validate(IPluginExecutionContext context);

        #endregion Public Methods
    }
}