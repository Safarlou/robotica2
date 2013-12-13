using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldModel
{
    enum RobotType { Transport, Guard }
    class Robot : Object
    {
        private RobotType Type { get; private set; }
    }
}
