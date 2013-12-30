
namespace WorldProcessing.Representation
{
    /// <summary>
    /// Enum defining the different types of robots; transport or guard.
    /// </summary>
    public enum RobotType { Transport, Guard }

    /// <summary>
    /// A class representing a robot in our WorldModel.
    /// </summary>
    public class Robot
    {
        public RobotType Type { get; private set; }
    }
}
