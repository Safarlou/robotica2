
namespace WorldProcessing.Representation
{
    /// <summary>
    /// Enum defining different types of obstacles.
    /// </summary>
    public enum ObstacleType { Wall, Block }

    /// <summary>
    /// An obstacle is an object defined by a Polygon, so it can have 
    /// any shape.
    /// </summary>
    public class Obstacle : Object
	{
		public ObstacleType Type { get; private set; }
        public Polygon Polygon { get; private set; }

		public Obstacle(ObstacleType type, Polygon polygon)
		{
			Type = type;
			Polygon = polygon;
		}
    }
}
