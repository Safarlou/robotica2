using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WorldProcessing.Representation
{
    /// <summary>
    /// Polygon is a simple list of vectors (ie. points) defining a certain shape.
    /// </summary>
    public class Polygon
    {
        public List<Point> Points { get; protected set; }

        public Polygon(List<Point> points)
        {
			Points = points;
        }
    }
}
