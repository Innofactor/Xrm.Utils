namespace Innofactor.Xrm.Utils.Workflow
{
    using System;
    using System.Activities;
    using Innofactor.Xrm.Utils.Common.Extensions;

    public abstract class ActivityBase : CodeActivity
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityBase" /> class. Will use refrected name of the calling plugin.
        /// </summary>
        public ActivityBase()
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public abstract void Execute(ActivityContainer container);

        #endregion Public Methods

        #region Protected Methods

        protected override void Execute(CodeActivityContext context)
        {
            var container = new ActivityContainer(context);

            try
            {
                new Action<ActivityContainer>(Execute).Invoke(container);
            }
            catch (Exception ex)
            {
                container.Log(ex);
                throw;
            }
        }

        #endregion Protected Methods
    }
}