using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.src.Representation
{
    /// <summary>
    /// Enum defining different types of obstacles.
    /// </summary>
    public enum ObstacleType { Block }

    /// <summary>
    /// An obstacle is an object defined by a Polygon, so it can have 
    /// any shape.
    /// </summary>
    public class Obstacle : Object
    {
        public Polygon Polygon { get; private set; }
        public ObstacleType Type { get; private set; }
    }
}
