
namespace WorldProcessing.Representation
{
	/// <summary>
	/// An object representing a goal in the WorldModel.
	/// </summary>
	public class Goal : Object
	{
		public Goal(System.Windows.Point point)
		{
			Position = point;
		}

		public override Constants.ObjectType ObjectType
		{
			get { return Constants.ObjectType.Goal; }
		}
	}
}
