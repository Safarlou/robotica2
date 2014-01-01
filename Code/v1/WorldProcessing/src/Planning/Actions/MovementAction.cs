using System.Windows;

namespace WorldProcessing.Planning
{
    /// <summary>
    /// An action describing a simple movement action by a robot.
    /// </summary>
    public class MovementAction : Action
    {
        public Vector Position { get; private set; }

        public MovementAction(Vector position)
        {
            this.Position = position;
        }
    }
}
