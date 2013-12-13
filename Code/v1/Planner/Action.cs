using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Planning
{
    enum ActionState {Due, InProgress, Complete}
    abstract class Action
    {
        public ActionState State { get; protected set; }
        public event EventHandler ActionStateChanged;

        public Action()
        {
            this.State = ActionState.Due;
        }

        public virtual void Start()
        {
            this.State = ActionState.InProgress;
            OnActionStateChanged(new EventArgs());
        }

        public virtual void End()
        {
            this.State = ActionState.Complete;
            OnActionStateChanged(new EventArgs());
        }

        public virtual void OnActionStateChanged(EventArgs e)
        {
            if (ActionStateChanged != null)
            {
                ActionStateChanged(this, e);
            }
        }
    }
}
