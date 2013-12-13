using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldModel;

namespace Planning
{
    class WaitAction : Action
    {
        public Action Dependency { get; private set; }

        public override void Start()
        {
            if (Dependency.State == ActionState.Complete)
            {
                this.State = ActionState.Complete;
            }
            Dependency.ActionStateChanged += DependencyStateListener;
        }

        private void DependencyStateListener(object sender, EventArgs e)
        {
            Action action = (Action)sender;
            if (action.State == ActionState.Complete)
            {
                this.State = ActionState.Complete;
            }
        }
    }
}
