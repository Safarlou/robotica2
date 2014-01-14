using System;

namespace WorldProcessing.src.Planning.Actions
{
    /// <summary>
    /// An action that waits on the completion of another action (Dependency).
    /// Used in case the transport robot has to wait until clearance by the guard robot.
    /// </summary>
    public class WaitAction : Action
    {
        public WaitAction()
        {
            // Necessary, will get NullReferenceExceptions otherwise
			this.Type = ActionType.Wait;
        }
    }
}
