using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.src.Planning
{
    /// <summary>
    /// An action that waits on the completion of another action (Dependency).
    /// Used in case the transport robot has to wait until clearance by the guard robot.
    /// </summary>
    public class WaitAction : Action
    {
        public Action Dependency;

        public WaitAction(Action dependency)
        {
            // Necessary, will get NullReferenceExceptions otherwise
            this.Dependency = dependency;
        }

        public override void Start()
        {
            base.Start();
            if (Dependency.State == ActionState.Complete)
                this.State = ActionState.Complete;
            //Register event listener
            Dependency.ActionStateChanged += DependencyStateListener;
        }

        private void DependencyStateListener(object sender, EventArgs e)
        {
            if (((Action)sender).State == ActionState.Complete)
                this.State = ActionState.Complete;
        }
    }
}
