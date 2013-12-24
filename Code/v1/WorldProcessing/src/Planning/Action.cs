using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.Planning
{
    /// <summary>
    /// An enumeration describing the state of an action.
    /// Due: action has yet to be executed.
    /// InProgress: action is currently being executed.
    /// Complete: action is completely executed.
    /// </summary>
    public enum ActionState { Due, InProgress, Complete}

    /// <summary>
    /// Abstract class providing basic functionality for actions.
    /// </summary>
    public abstract class Action
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

        private void OnActionStateChanged(EventArgs e)
        {
            if (ActionStateChanged != null)
            {
                ActionStateChanged(this, e);
            }
        }
    }
}
