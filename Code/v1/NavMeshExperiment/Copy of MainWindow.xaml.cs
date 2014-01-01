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

namespace NavMeshExperiment
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindowCopy : Window
	{
		private List<PolygonWithNeighbors> finalpolygons;

		public MainWindowCopy()
		{
			InitializeComponent();

			canvas.Background = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color()); // to make canvas clickable. weird shit.

			Test();
		}

		public void Test2()
		{
			var poly1 = new PolygonWithNeighbors();
			poly1.polygon.Points.Add(new System.Windows.Point(10, 10));
			poly1.polygon.Points.Add(new System.Windows.Point(20, 20));
			poly1.polygon.Points.Add(new System.Windows.Point(10, 30));
			poly1.polygon.Stroke = System.Windows.Media.Brushes.Black;
			//canvas.Children.Add(poly1.polygon);

			var poly2 = new PolygonWithNeighbors();
			poly2.polygon.Points.Add(new System.Windows.Point(50, 50));
			poly2.polygon.Points.Add(new System.Windows.Point(20, 20));
			poly2.polygon.Points.Add(new System.Windows.Point(10, 30));
			poly2.polygon.Stroke = System.Windows.Media.Brushes.Black;
			//canvas.Children.Add(poly2.polygon);

			var poly3 = Consolidate(poly1, poly2);
			poly3.polygon.Stroke = System.Windows.Media.Brushes.Black;
			canvas.Children.Add(poly3.polygon);

			Console.WriteLine(IsConvex(poly3));
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
			geo.AddPoint(110, 50);
			geo.AddPoint(160, 50);
			geo.AddPoint(160, 100);
			geo.AddPoint(110, 100);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(120, 60);

			// square 3
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
			geo.AddPoint(55, 150);
			geo.AddPoint(100, 155);
			geo.AddPoint(95, 300);
			geo.AddPoint(50, 295);

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
					if (poly == poly2 || poly.neighbors.Contains(poly2) || poly2.neighbors.Contains(poly))
						continue;

					if (Neighbors(poly, poly2))
					{
						poly.neighbors.Add(poly2);
						poly2.neighbors.Add(poly);
					}
				}

			Console.WriteLine(polygons.Count);

			// this hack rounds all points to whole numbers because Clipper lib handles only ints and feeding it doubles introduces rounding errors
			// unfortunately this introduces mistakes when checking for convexity...
			//foreach (var polygon in polygons)
			//	polygon.polygon.Points = new PointCollection((from point in polygon.polygon.Points select new System.Windows.Point(Math.Round(point.X), Math.Round(point.Y))));


			//sort polygons by size/pointcount (smallest first). probably need to do each iteration

			//consolidate
			while (ConsolidationStep(polygons)) ;

			// draw
			foreach (PolygonWithNeighbors poly in polygons)
			{
				poly.polygon.Stroke = System.Windows.Media.Brushes.Black;
				poly.polygon.Fill = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color()); // to make poly clickable. weird shit.
				canvas.Children.Add(poly.polygon);
			}

			finalpolygons = polygons;
		}

		private static bool Neighbors(PolygonWithNeighbors poly, PolygonWithNeighbors poly2)
		{
			int sharedVertices = 0;
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 3; j++)
					if (poly.polygon.Points[i].X == poly2.polygon.Points[j].X && poly.polygon.Points[i].Y == poly2.polygon.Points[j].Y)
						if (++sharedVertices > 1)
							return true;
			return false;
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
			bool p2set = false;

			foreach (System.Windows.Point point in newneighbor.polygon.Points)
				foreach (System.Windows.Point point2 in hypothetical.polygon.Points)
					if (point == point2)
					{
						if (!p1set)
						{
							p1 = point;
							p1set = true;
						}
						else if (!p2set)
						{
							p2 = point;
							p2set = true;
						}
					}

			//Console.WriteLine("hypothetical points:");
			//foreach (var point in hypothetical.polygon.Points)
			//	Console.WriteLine(point);

			//Console.WriteLine("neighbor points:");
			//foreach (var point in newneighbor.polygon.Points)
			//	Console.WriteLine(point);

			//Console.WriteLine("intersecting points:");
			//var intersection = newneighbor.polygon.Points.Intersect(hypothetical.polygon.Points);

			//foreach (System.Windows.Point point in intersection)
			//{
			//	Console.WriteLine(point);
			//}

			//var line = new Line();
			//line.X1 = p1.X;
			//line.Y1 = p1.Y;
			//line.X2 = p2.X;
			//line.Y2 = p2.Y;
			//line.X1 = Centroid(newneighbor).X;
			//line.Y1 = Centroid(newneighbor).Y;
			//line.X2 = Centroid(hypothetical).X;
			//line.Y2 = Centroid(hypothetical).Y;
			//line.StrokeThickness = 5;

			//line.Stroke = System.Windows.Media.Brushes.Black;

			//canvas.Children.Add(line);

			if (!p1set || !p2set)
				return false;

			//Console.WriteLine(p1);
			//Console.WriteLine(p2);

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

			var lines1 = new List<Line>();

			for (int i = 0; i < poly1.polygon.Points.Count; i++)
			{
				Line line = new Line();
				int c = poly1.polygon.Points.Count;
				line.X1 = poly1.polygon.Points[i].X;
				line.Y1 = poly1.polygon.Points[i].Y;
				line.X2 = poly1.polygon.Points[mod(i + 1, c)].X;
				line.Y2 = poly1.polygon.Points[mod(i + 1, c)].Y;
				lines1.Add(line);
			}

			var lines2 = new List<Line>();

			for (int i = 0; i < poly2.polygon.Points.Count; i++)
			{
				Line line = new Line();
				int c = poly2.polygon.Points.Count;
				line.X1 = poly2.polygon.Points[i].X;
				line.Y1 = poly2.polygon.Points[i].Y;
				line.X2 = poly2.polygon.Points[mod(i + 1, c)].X;
				line.Y2 = poly2.polygon.Points[mod(i + 1, c)].Y;
				lines2.Add(line);
			}

			var lines = new List<Line>();

			foreach (var line in lines1.Concat(lines2))
				if (!LineContain(lines1, line) || !LineContain(lines2, line))
					lines.Add(line);

			result.polygon.Points.Add(new System.Windows.Point(lines[0].X1, lines[0].Y1));

			while (lines.Count() > 0)
			{
				var prev = result.polygon.Points.Last();
				var next = lines.Find(new Predicate<Line>(x => x.X1 == prev.X && x.Y1 == prev.Y));
				if (next != null)
				{
					result.polygon.Points.Add(new System.Windows.Point(next.X2, next.Y2));
				}
				else
				{
					next = lines.Find(new Predicate<Line>(x => x.X2 == prev.X && x.Y2 == prev.Y));
					if (next != null)
						result.polygon.Points.Add(new System.Windows.Point(next.X1, next.Y1));
					else
						break;
				}
				lines.Remove(next);
			}

			result.neighbors = poly1.neighbors.Concat(poly2.neighbors).GroupBy(item => item.GetHashCode()).Select(group => group.First()).ToList();
			result.neighbors.Remove(poly1);
			result.neighbors.Remove(poly2);

			return result;

			//var points1 = new List<IntPoint>();
			//foreach (System.Windows.Point point in poly1.polygon.Points)
			//	points1.Add(new IntPoint(point.X, point.Y));

			//var points2 = new List<IntPoint>();
			//foreach (System.Windows.Point point in poly2.polygon.Points)
			//	points2.Add(new IntPoint(point.X, point.Y));

			//Clipper c = new Clipper();
			//c.AddPath(points1, PolyType.ptSubject, true);
			//c.AddPath(points2, PolyType.ptSubject, true);

			//var solution = new List<List<IntPoint>>();
			//c.Execute(ClipType.ctUnion, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

			//foreach (List<IntPoint> poly in solution)
			//	foreach (IntPoint point in poly)
			//		result.polygon.Points.Add(new System.Windows.Point(point.X, point.Y));

			////foreach (PolygonWithNeighbors neighbor in poly1.neighbors)
			////{
			////	if (neighbor != poly1 && neighbor != poly2)
			////	{
			////		result.neighbors.Add(neighbor);
			////	}
			////}

			////foreach (PolygonWithNeighbors neighbor in poly2.neighbors)
			////{
			////	if (neighbor != poly1 && neighbor != poly2 && !result.neighbors.Contains(neighbor))
			////	{
			////		result.neighbors.Add(neighbor);
			////	}
			////}

			//// above is a more verbose version of the below. I'm certain the above works, as to the below I have small doubts... -Marein

			//result.neighbors = poly1.neighbors.Concat(poly2.neighbors).GroupBy(item => item.GetHashCode()).Select(group => group.First()).ToList();
			//result.neighbors.Remove(poly1);
			//result.neighbors.Remove(poly2);

			//return result;
		}

		private bool LineContain(List<Line> lines, Line line)
		{
			foreach (var line2 in lines)
				if ((line2.X1 == line.X1 && line2.X2 == line.X2 && line2.Y1 == line.Y1 && line2.Y2 == line.Y2)
					|| (line2.X1 == line.X2 && line2.X2 == line.X1 && line2.Y1 == line.Y2 && line2.Y2 == line.Y1))
					return true;
			return false;
		}

		private bool IsConvex(PolygonWithNeighbors poly)
		{
			int c = poly.polygon.Points.Count;
			int sign = 0; // the sign of zcrossproduct (pos or neg) must be the same for all angles in the polygon
			for (int i = 0; i < c; i++)
			{
				System.Windows.Point p0 = poly.polygon.Points[mod((i - 1), c)];
				System.Windows.Point p1 = poly.polygon.Points[i];
				System.Windows.Point p2 = poly.polygon.Points[mod((i + 1), c)];

				var z = zcrossproduct(p0, p1, p2);

				if (z == 0) continue;

				if (sign == 0)
					sign = Math.Sign(z);
				else if (Math.Sign(z) != sign)
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

		private void canvas_MouseMove(object sender, MouseEventArgs e)
		{

			foreach (PolygonWithNeighbors poly in finalpolygons)
			{
				poly.polygon.Fill = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color());
				poly.polygon.Stroke = System.Windows.Media.Brushes.Black;
				poly.polygon.StrokeThickness = 1;
			}

			foreach (PolygonWithNeighbors poly in finalpolygons)
				if (poly.polygon.IsMouseOver)
				{
					text.Text = "";

					poly.polygon.Fill = System.Windows.Media.Brushes.LightBlue;

					text.Text += "neighbors: " + poly.neighbors.Count + "\n";

					int n = 0;
					foreach (PolygonWithNeighbors neighbor in poly.neighbors)
					{
						text.Text += "neighbor " + n + "\n";

						switch (n)
						{
							case 0: neighbor.polygon.Fill = System.Windows.Media.Brushes.LightSkyBlue; break;
							case 1: neighbor.polygon.Fill = System.Windows.Media.Brushes.LightGreen; break;
							case 2: neighbor.polygon.Fill = System.Windows.Media.Brushes.LightYellow; break;
							case 3: neighbor.polygon.Fill = System.Windows.Media.Brushes.LightPink; break;
							case 4: neighbor.polygon.Fill = System.Windows.Media.Brushes.LightCyan; break;
						}

						text.Text += "   color: ";
						switch (n)
						{
							case 0: text.Text += "Blue\n"; break;
							case 1: text.Text += "Green\n"; break;
							case 2: text.Text += "Yellow\n"; break;
							case 3: text.Text += "Pink\n"; break;
							case 4: text.Text += "Cyan\n"; break;
						}

						PolygonWithNeighbors hypothetical = Consolidate(poly, neighbor);

						hypothetical.polygon.Fill = System.Windows.Media.Brushes.OrangeRed;
						canvas.Children.Add(hypothetical.polygon);

						text.Text += "   newneighbors: " + hypothetical.neighbors.Count + "\n";

						foreach (var newneighbor in hypothetical.neighbors)
						{
							newneighbor.polygon.StrokeThickness = 3;
							//newneighbor.polygon.Fill = System.Windows.Media.Brushes.Black;
							if (!canvas.Children.Contains(newneighbor.polygon))
								canvas.Children.Add(newneighbor.polygon);
						}

						text.Text += "   reachable: " + IsReachable(hypothetical) + "\n";
						text.Text += "   convex: " + IsConvex(hypothetical) + "\n";
						//text.Text += "   consolidable: " + (IsReachable(hypothetical) && IsConvex(hypothetical)) + "\n";

						n++;
					}
				}
		}
	}
}
