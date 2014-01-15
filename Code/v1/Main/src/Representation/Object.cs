
namespace WorldProcessing.Representation
{
    /// <summary>
    /// An abstract class defining some sort of entity or object in the WorldModel.
    /// Every object in our WorldModel has an orientation (especially important when the object is a robot)
    /// and a position.
    /// </summary>
    public abstract class Object
    {
		public double Orientation { get; protected set; } //radians or some shit
        public System.Windows.Point Position { get; protected set; }
		public abstract Constants.ObjectType ObjectType { get; }

        public Object()
        {
            // Not sure if needed
			this.Position = new System.Windows.Point();
        }
    }
}
