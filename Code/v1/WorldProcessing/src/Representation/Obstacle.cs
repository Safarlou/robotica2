
namespace WorldProcessing.Representation
{
	/// <summary>
	/// An obstacle is an object defined by a Polygon, so it can have 
	/// any shape.
	/// </summary>
	public abstract class Obstacle : Object
	{
		public Polygon Polygon { get; private set; }

		public Obstacle(Polygon polygon)
		{
			Polygon = polygon;
			Position = polygon.Centroid;
		}
	}

	public class Wall : Obstacle
	{
		public Wall(Polygon polygon) : base(polygon) { }

		public override Constants.ObjectType ObjectType
		{
			get { return Constants.ObjectType.Wall; }
		}
	};

	public class Block : Obstacle
	{
		public Block(Polygon polygon) : base(polygon) {}

		public override Constants.ObjectType ObjectType
		{
			get { return Constants.ObjectType.Block; }
		}
	};
}
