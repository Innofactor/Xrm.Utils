namespace Innofactor.Xrm.Utils.Workflow
{
    using System;
    using System.Activities;

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

        protected override void Execute(CodeActivityContext context) =>
            new Action<ActivityContainer>(Execute).Invoke(new ActivityContainer(context));

        #endregion Protected Methods
    }
}