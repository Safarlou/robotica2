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

		public double Size
		{
			get
			{
				var points = new List<System.Windows.Point>();
				foreach (var point in Points)
					points.Add(point);

				points.Add(points[0]);
				return Math.Abs(points.Take(points.Count - 1)
				   .Select((p, i) => (points[i + 1].X - p.X) * (points[i + 1].Y + p.Y))
				   .Sum() / 2);
			}
		}

		public bool IsConvex
		{
			get
			{
				int c = Points.Count;
				int[] signs = new int[c]; // the sign of zcrossproduct (pos or neg) must be the same for all angles in the polygon
				for (int i = 0; i < c; i++)
				{
					System.Windows.Point p0 = Points[Util.Maths.Mod((i - 1), c)];
					System.Windows.Point p1 = Points[i];
					System.Windows.Point p2 = Points[Util.Maths.Mod((i + 1), c)];

					signs[i] = Math.Sign(Util.Maths.Zcrossproduct(p0, p1, p2));
				}

				return signs.All(x => x >= 0) || signs.All(x => x <= 0);
			}
		}

		public List<Edge> Edges
		{
			get
			{
				var edges = new List<Edge>();
				int c = Points.Count;
				for (int i = 0; i < c; i++)
					edges.Add(new Edge(Points[i], Points[Util.Maths.Mod(i + 1, c)]));
				return edges;
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
	}
}
