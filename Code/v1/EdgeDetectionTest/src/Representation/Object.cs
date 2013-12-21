using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Model.Representation
{
    /// <summary>
    /// An abstract class defining some sort of entity or object in the WorldModel.
    /// Every object in our WorldModel has an orientation (especially important when the object is a robot)
    /// and a position.
    /// </summary>
    abstract class Object
    {
        public double Orientation { get; private set; } //radians or some shit
        public Vector Position { get; private set; }

        public Object()
        {
            // Not sure if needed
            this.Position = new Vector();
        }
    }
}
