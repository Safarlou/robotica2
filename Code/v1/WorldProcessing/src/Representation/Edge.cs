using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using WorldProcessing.Planning;

namespace WorldProcessing.Representation
{
	public class Edge
	{
		public Point p1;
		public Point p2;

		public Point Center
		{
			get
			{
				return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
			}
		}

		public Edge(Point point1, Point point2)
		{
			this.p1 = point1;
			this.p2 = point2;
		}
	}
}
