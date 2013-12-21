using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WorldProcessing.src.Planning
{
    /// <summary>
    /// An action describing a simple movement action by a robot.
    /// </summary>
    public class MovementAction : Action
    {
        public Vector Position { get; private set; }

        public MovementAction(Vector position)
        {
            this.Position = position;
        }
    }
}
