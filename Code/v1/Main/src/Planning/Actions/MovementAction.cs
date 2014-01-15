
namespace WorldProcessing.Planning.Actions
{
	/// <summary>
	/// An action describing a simple movement action by a robot.
	/// </summary>
	public class MovementAction : Action
	{
		public System.Windows.Point Position { get; private set; }

		public MovementAction(System.Windows.Point position)
		{
			this.Position = position;
			this.Type = ActionType.Move;
		}
	}
}
