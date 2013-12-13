using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Planning
{
    class MovementAction : Action
    {
        // The position is the location to be reached after this action.
        public Vector Position { get; private set; }

        public MovementAction(Vector position)
        {
            this.Position = position;
        }
    }
}
