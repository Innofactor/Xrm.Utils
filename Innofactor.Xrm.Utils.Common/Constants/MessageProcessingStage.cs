namespace Innofactor.Xrm.Utils.Common.Constants
{
    /// <summary>Message processing stages for plugin steps</summary>
    public static class MessageProcessingStage
    {
        #region Public Fields

        public const int AfterMainOperationInsideTransaction = 40;
        public const int AfterMainOperationOutsideTransaction = 50;
        public const int BeforeMainOperationInsideTransaction = 20;
        public const int BeforeMainOperationOutsideTransaction = 10;
        public const int MainOperation = 30;

        #endregion Public Fields
    }
}