using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Model.Planning
{
    /// <summary>
    /// An action describing a simple movement action by a robot.
    /// </summary>
    class MovementAction : Action
    {
        public Vector Position { get; private set; }

        public MovementAction(Vector position)
        {
            this.Position = position;
        }
    }
}
