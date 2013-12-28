using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Data;
using ClipperLib;

namespace NavMeshExperiment
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			Test();
		}

		public void Test()
		{
			InputGeometry geo = new InputGeometry();

			int c;

			// boundaries
			geo.AddPoint(0, 0);
			geo.AddPoint(300, 0);
			geo.AddPoint(300, 300);
			geo.AddPoint(0, 300);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			// square 1
			geo.AddPoint(45, 50);
			geo.AddPoint(100, 45);
			geo.AddPoint(105, 100);
			geo.AddPoint(50, 105);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(60, 60);

			// square 2
			geo.AddPoint(150, 200);
			geo.AddPoint(200, 200);
			geo.AddPoint(200, 250);
			geo.AddPoint(150, 250);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(160, 210);

			// bar 1
			geo.AddPoint(50, 150);
			geo.AddPoint(100, 150);
			geo.AddPoint(100, 300);
			geo.AddPoint(50, 300);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(60, 160);

			// bar 2
			geo.AddPoint(200, 0);
			geo.AddPoint(250, 0);
			geo.AddPoint(250, 150);
			geo.AddPoint(200, 150);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(210, 10);

			Mesh mesh = new Mesh();
			mesh.Behavior.Quality = true;
			mesh.Behavior.MinAngle = 20;
			mesh.Triangulate(geo);

			var polygons = new List<PolygonWithNeighbors>();

			// convert to polygon
			foreach (Triangle triangle in mesh.Triangles)
			{
				var poly = new PolygonWithNeighbors();
				for (int i = 0; i < 3; i++)
					poly.polygon.Points.Add(new System.Windows.Point(triangle.GetVertex(i).X, triangle.GetVertex(i).Y));
				polygons.Add(poly);
			}

			// find neighbors
			foreach (PolygonWithNeighbors poly in polygons)
				foreach (PolygonWithNeighbors poly2 in polygons)
				{
					if (poly == poly2)
						continue;

					int sharedVertices = 0;
					for (int i = 0; i < 3; i++)
						for (int j = 0; j < 3; j++)
						{
							if (poly.polygon.Points[i].X == poly2.polygon.Points[j].X && poly.polygon.Points[i].Y == poly2.polygon.Points[j].Y)
								sharedVertices++;
						}

					if (sharedVertices < 2)
						continue;

					poly.neighbors.Add(poly2);
				}

			//consolidate
			while (ConsolidationStep(polygons)) ;

			// draw
			foreach (PolygonWithNeighbors poly in polygons)
			{
				poly.polygon.Stroke = System.Windows.Media.Brushes.Black;
				canvas.Children.Add(poly.polygon);
			}
		}

		private bool ConsolidationStep(List<PolygonWithNeighbors> polygons)
		{
			foreach (PolygonWithNeighbors poly in polygons)
				foreach (PolygonWithNeighbors neighbor in poly.neighbors)
				{
					PolygonWithNeighbors hypothetical = Consolidate(poly, neighbor);

					if (IsReachable(hypothetical) && IsConvex(hypothetical))
					{
						polygons.Remove(poly);
						polygons.Remove(neighbor);
						polygons.Add(hypothetical);
						foreach (PolygonWithNeighbors newneighbor in hypothetical.neighbors)
						{
							newneighbor.neighbors.Remove(poly);
							newneighbor.neighbors.Remove(neighbor);
							newneighbor.neighbors.Add(hypothetical);
						}
						return true; // break out because the iterator has become invalid: restart iteration
					}
				}

			return false;
		}

		private bool IsReachable(PolygonWithNeighbors hypothetical)
		{
			foreach (PolygonWithNeighbors newneighbor in hypothetical.neighbors)
				if (!CanReach(newneighbor, hypothetical))
					return false;
			return true;
		}

		private bool CanReach(PolygonWithNeighbors newneighbor, PolygonWithNeighbors hypothetical)
		{
			System.Windows.Point p1 = new System.Windows.Point();
			System.Windows.Point p2 = new System.Windows.Point();
			bool p1set = false;

			foreach (System.Windows.Point point in newneighbor.polygon.Points)
				foreach (System.Windows.Point point2 in hypothetical.polygon.Points)
					if (point == point2)
					{
						if (!p1set)
						{
							p1 = point;
							p1set = true;
						}
						else
							p2 = point;
					}

			return LineIntersectsLine(p1, p2, Centroid(newneighbor), Centroid(hypothetical));
		}

		private static System.Windows.Point Centroid(PolygonWithNeighbors polygon)
		{
			var poly = polygon.polygon.Points.ToArray();

			double accumulatedArea = 0.0f;
			double centerX = 0.0f;
			double centerY = 0.0f;

			for (int i = 0, j = poly.Count() - 1; i < poly.Count(); j = i++)
			{
				double temp = poly[i].X * poly[j].Y - poly[j].X * poly[i].Y;
				accumulatedArea += temp;
				centerX += (poly[i].X + poly[j].X) * temp;
				centerY += (poly[i].Y + poly[j].Y) * temp;
			}

			accumulatedArea *= 3f;
			return new System.Windows.Point(centerX / accumulatedArea, centerY / accumulatedArea);
		}

		private static bool LineIntersectsLine(System.Windows.Point l1p1, System.Windows.Point l1p2, System.Windows.Point l2p1, System.Windows.Point l2p2)
		{
			double q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
			double d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

			if (d == 0)
			{
				return false;
			}

			double r = q / d;

			q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
			double s = q / d;

			if (r < 0 || r > 1 || s < 0 || s > 1)
			{
				return false;
			}

			return true;
		}

		private PolygonWithNeighbors Consolidate(PolygonWithNeighbors poly1, PolygonWithNeighbors poly2)
		{
			PolygonWithNeighbors result = new PolygonWithNeighbors();

			var points1 = new List<IntPoint>();
			foreach (System.Windows.Point point in poly1.polygon.Points)
				points1.Add(new IntPoint(point.X, point.Y));

			var points2 = new List<IntPoint>();
			foreach (System.Windows.Point point in poly2.polygon.Points)
				points2.Add(new IntPoint(point.X, point.Y));

			Clipper c = new Clipper();
			c.AddPath(points1, PolyType.ptSubject, true);
			c.AddPath(points2, PolyType.ptSubject, true);

			var solution = new List<List<IntPoint>>();
			c.Execute(ClipType.ctUnion, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

			foreach (List<IntPoint> poly in solution)
				foreach (IntPoint point in poly)
					result.polygon.Points.Add(new System.Windows.Point(point.X, point.Y));

			//foreach (PolygonWithNeighbors neighbor in poly1.neighbors)
			//{
			//	if (neighbor != poly1 && neighbor != poly2)
			//	{
			//		result.neighbors.Add(neighbor);
			//	}
			//}

			//foreach (PolygonWithNeighbors neighbor in poly2.neighbors)
			//{
			//	if (neighbor != poly1 && neighbor != poly2 && !result.neighbors.Contains(neighbor))
			//	{
			//		result.neighbors.Add(neighbor);
			//	}
			//}

			// above is a more verbose version of the below. I'm certain the above works, as to the below I have small doubts... -Marein

			result.neighbors = poly1.neighbors.Concat(poly2.neighbors).GroupBy(item => item.GetHashCode()).Select(group => group.First()).ToList();
			result.neighbors.Remove(poly1);
			result.neighbors.Remove(poly2);

			return result;
		}

		private bool IsConvex(PolygonWithNeighbors poly)
		{
			int c = poly.polygon.Points.Count;
			int kind = 0; // the kind of zcrossproduct (pos or neg) must be the same for all angles in the polygon
			for (int i = 0; i < c; i++)
			{
				System.Windows.Point p0 = poly.polygon.Points[mod((i - 1), c)];
				System.Windows.Point p1 = poly.polygon.Points[i];
				System.Windows.Point p2 = poly.polygon.Points[mod((i + 1), c)];

				if (kind == 0)
					kind = Math.Sign(zcrossproduct(p0, p1, p2));
				else if (Math.Sign(zcrossproduct(p0, p1, p2)) != kind)
					return false;
			}

			return true;
		}

		private int mod(int x, int m) // % is remainder which doesn't have the same effect as mod for negative x, hence this function
		{
			return (x % m + m) % m;
		}

		private double zcrossproduct(System.Windows.Point p0, System.Windows.Point p1, System.Windows.Point p2)
		{
			double dx1 = p1.X - p0.X;
			double dy1 = p1.Y - p0.Y;
			double dx2 = p2.X - p1.X;
			double dy2 = p2.Y - p1.Y;

			return dx1 * dy2 - dy1 * dx2;
		}

		private bool Clockwise(PolygonWithNeighbors poly)
		{
			int c = poly.polygon.Points.Count;
			double sum = 0;
			for (int i = 0; i < c; i++)
			{
				System.Windows.Point p1 = poly.polygon.Points[i];
				System.Windows.Point p2 = poly.polygon.Points[mod((i + 1), c)];

				sum += (p2.X - p1.X) * (p2.Y + p1.Y);
			}

			return sum >= 0;
		}
	}
}
