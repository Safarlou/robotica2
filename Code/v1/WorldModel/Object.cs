using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WorldModel
{
    abstract class Object
    {
        public double Orientation { get; private set; } //radians ?
        public Vector Position { get; private set; }

        public Object()
        {
            Position = new Vector();
        }
    }
}
