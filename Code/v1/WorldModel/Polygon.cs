using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WorldModel
{
    class Polygon
    {
        protected List<Vector> points { get; private set; }

        public Polygon() 
        {
            points = new List<Vector>();
        }
    }
}
