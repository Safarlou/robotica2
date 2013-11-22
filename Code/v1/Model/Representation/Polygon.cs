﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Model.Representation
{
    /// <summary>
    /// Polygon is a simple list of vectors (ie. points) defining a certain shape.
    /// </summary>
    class Polygon : Object
    {
        protected List<Vector> Points { get; private set; }

        public Polygon()
        {
            Points = new List<Vector>();
        }
    }
}
