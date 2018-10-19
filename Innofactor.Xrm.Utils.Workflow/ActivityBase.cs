namespace Innofactor.Xrm.Utils.Workflow
{
    using System;
    using System.Activities;

    public abstract class ActivityBase : CodeActivity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityBase" /> class. Will use refrected name of the calling plugin.
        /// </summary>
        public ActivityBase()
        {
        }

        public abstract void Execute(ActivityContainer container);

        protected override void Execute(CodeActivityContext context) =>
            new ActivityContainer(context)
            {
                Action = new Action<ActivityContainer>(Execute)
            }.Execute();
    }
}