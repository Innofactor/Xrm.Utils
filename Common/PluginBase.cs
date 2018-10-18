namespace Innofactor.Xrm.DevUtils.Common
{
    using System;
    using System.Diagnostics;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// MS CRM Base Plugin. Makes usage of <see cref="PluginContainer"/> more easy.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBase"/> class. Will use refrected name of the calling plugin.
        /// </summary>
        public PluginBase()
            : base()
        {
            name = (new StackTrace()).GetFrame(1).GetMethod().ReflectedType.Name;
        }

        /// <summary>
        /// Default entry point for CRM plugin
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void Execute(IServiceProvider serviceProvider) =>
            new PluginContainer(serviceProvider)
            {
                Validator = new Predicate<IPluginExecutionContext>(Validate),
                Action = new Action<PluginContainer>(Execute)
            }
            .Execute();

        /// <summary>
        /// Main entry point for the plugin
        /// </summary>
        /// <param name="container"></param>
        public abstract void Execute(PluginContainer container);

        /// <summary>
        /// Method to validate plugin's execution context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract bool Validate(IPluginExecutionContext context);
    }
}