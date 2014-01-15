using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace WorldProcessing.Representation
{
	/// <summary>
	/// Polygon is a simple list of points defining a certain shape.
	/// </summary>
	public class Polygon
	{
		public List<Point> Points { get; protected set; }

		public Point Centroid
		{
			get
			{
				double accumulatedArea = 0.0f;
				double centerX = 0.0f;
				double centerY = 0.0f;

				for (int i = 0, j = Points.Count - 1; i < Points.Count; j = i++)
				{
					double temp = Points[i].X * Points[j].Y - Points[j].X * Points[i].Y;
					accumulatedArea += temp;
					centerX += (Points[i].X + Points[j].X) * temp;
					centerY += (Points[i].Y + Points[j].Y) * temp;
				}

				accumulatedArea *= 3f;
				return new System.Windows.Point(centerX / accumulatedArea, centerY / accumulatedArea);
			}
		}

		public Polygon()
		{
			Points = new List<Point>();
		}

		public Polygon(List<Point> points)
		{
			Points = points;
		}

		public Polygon(Emgu.CV.Seq<System.Drawing.Point> points)
		{
			Points = (from p in points select new Point(p.X,p.Y)).ToList();
		}

		public Polygon(System.Drawing.PointF[] points)
		{
			Points = (from p in points select new Point(p.X, p.Y)).ToList();
		}
	}
}
