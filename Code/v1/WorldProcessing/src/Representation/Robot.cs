
namespace WorldProcessing.Representation
{
	/// <summary>
	/// A class representing a robot in our WorldModel.
	/// </summary>
	public class Robot : Object
	{
		public Robot() { }

		public Robot(System.Windows.Point point)
		{
			this.Position = point;
		}

		public override Constants.ObjectType ObjectType
		{
			get { return Constants.ObjectType.Robot; }
		}
	}

	public class TransportRobot : Robot
	{
		public TransportRobot(System.Windows.Point point) : base(point) { }

		public TransportRobot(System.Windows.Point robotMarker, System.Windows.Point transportMarker)
		{
			Position = Util.Maths.Average(robotMarker, transportMarker);
			Orientation = Util.Maths.Angle(robotMarker, transportMarker);
		}

		public TransportRobot(Representation.Robot p1, Representation.Robot p2) : this(p1.Position, p2.Position) { }

		public override Constants.ObjectType ObjectType
		{
			get { return Constants.ObjectType.TransportRobot; }
		}
	}

	public class GuardRobot : Robot
	{
		public GuardRobot(System.Windows.Point point) : base(point) { }

		public GuardRobot(System.Windows.Point robotMarker, System.Windows.Point guardMarker)
		{
			Position = Util.Maths.Average(robotMarker, guardMarker);
			Orientation = Util.Maths.Angle(robotMarker, guardMarker);
		}

		public GuardRobot(Representation.Robot p1, Representation.Robot p2) : this(p1.Position, p2.Position) { }

		public override Constants.ObjectType ObjectType
		{
			get { return Constants.ObjectType.GuardRobot; }
		}
	}
}
