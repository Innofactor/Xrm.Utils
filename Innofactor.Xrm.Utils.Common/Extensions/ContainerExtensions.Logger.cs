﻿using Innofactor.Xrm.Utils.Common.Interfaces;

namespace Innofactor.Xrm.Utils.Common.Extensions
{
    public static partial class ContainerExtensions
    {
        #region Public Methods

        public static void EndSection(this IContainable container) =>
            container.Logger.EndSection();

        public static void StartSection(this IContainable container, string name) =>
            container.Logger.StartSection(name);

        #endregion Public Methods
    }
}