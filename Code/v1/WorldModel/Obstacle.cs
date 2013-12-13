using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldModel
{
    enum ObstacleType { Block }
    class Obstacle : Object
    {
        public Polygon Polygon;
        public ObstacleType Type { get; private set; }
    }
}
