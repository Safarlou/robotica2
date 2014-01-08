
namespace WorldProcessing.Representation
{
	/// <summary>
	/// A class representing a robot in our WorldModel.
	/// </summary>
	public abstract class Robot : Object { }
	public class TransportRobot : Robot
	{
		public TransportRobot(System.Windows.Point robotMarker, System.Windows.Point transportMarker)
		{
			Position = Util.Maths.Average(robotMarker, transportMarker);
			Orientation = Util.Maths.Angle(robotMarker, transportMarker);
		}

		public override Constants.ObjectType ObjectType
		{
			get { return Constants.ObjectType.TransportRobot; }
		}
	}
	public class GuardRobot : Robot
	{
		public GuardRobot(System.Windows.Point robotMarker, System.Windows.Point guardMarker)
		{
			Position = Util.Maths.Average(robotMarker, guardMarker);
			Orientation = Util.Maths.Angle(robotMarker, guardMarker);
		}

		public override Constants.ObjectType ObjectType
		{
			get { return Constants.ObjectType.GuardRobot; }
		}
	}
}
