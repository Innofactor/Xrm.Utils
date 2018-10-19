namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using Innofactor.Xrm.Utils.Common.Interfaces;

    public static partial class ContainerExtensions
    {
        public static void EndSection(this IExecutionContainer container) =>
            container.Logger.EndSection();

        public static void StartSection(this IExecutionContainer container, string name) =>
            container.Logger.StartSection(name);
    }
}