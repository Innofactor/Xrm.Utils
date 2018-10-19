using Innofactor.Xrm.Utils.Common.Interfaces;

namespace Innofactor.Xrm.Utils.Common.Extensions
{
    public static partial class ContainerExtensions
    {
        public static void EndSection(this IExecutionContainer container) =>
            container.Logger.EndSection();

        public static void StartSection(this IExecutionContainer container, string name) =>
            container.Logger.StartSection(name);
    }
}