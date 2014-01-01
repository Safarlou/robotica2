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
	public partial class MainWindow : Window
	{
		private List<PolygonWithNeighbors> polygons;

		private PointWithNeighbors start, destination;
		private List<PointWithNeighbors> path;
		private List<Line> pathlines = new List<Line>();
		private List<PointWithNeighbors> points = new List<PointWithNeighbors>();

		public MainWindow()
		{
			InitializeComponent();

			canvas.Background = System.Windows.Media.Brushes.Black; // to make canvas clickable. weird shit.

			Test();
		}

		private void Test()
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
			geo.AddPoint(70, 45);
			geo.AddPoint(72, 70);
			geo.AddPoint(50, 75);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(60, 60);

			// square 2
			geo.AddPoint(110, 50);
			geo.AddPoint(130, 50);
			geo.AddPoint(130, 70);
			geo.AddPoint(110, 70);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(120, 60);

			// square 3
			geo.AddPoint(120, 105);
			geo.AddPoint(135, 100);
			geo.AddPoint(140, 115);
			geo.AddPoint(125, 120);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(130, 115);

			// square 4
			geo.AddPoint(150, 200);
			geo.AddPoint(170, 200);
			geo.AddPoint(170, 220);
			geo.AddPoint(150, 220);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(160, 210);

			// square 5
			geo.AddPoint(175, 200);
			geo.AddPoint(195, 200);
			geo.AddPoint(195, 220);
			geo.AddPoint(175, 220);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(185, 210);

			// square 6
			geo.AddPoint(175, 225);
			geo.AddPoint(195, 225);
			geo.AddPoint(195, 245);
			geo.AddPoint(175, 245);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(185, 235);

			// bar 1
			geo.AddPoint(75, 140);
			geo.AddPoint(100, 140);
			geo.AddPoint(100, 290);
			geo.AddPoint(75, 290);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(80, 150);

			// bar 2
			geo.AddPoint(100, 140);
			geo.AddPoint(150, 140);
			geo.AddPoint(150, 165);
			geo.AddPoint(100, 165);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(110, 150);

			// bar 3
			geo.AddPoint(200, 10);
			geo.AddPoint(225, 10);
			geo.AddPoint(225, 160);
			geo.AddPoint(200, 160);

			c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);

			geo.AddHole(210, 20);

			Mesh mesh = new Mesh();
			mesh.Behavior.Quality = true;
			mesh.Behavior.MinAngle = 5;			// larger numbers create more triangles, sometimes good, sometimes bad...
			mesh.Triangulate(geo);

			polygons = ConvertToPolygons(mesh.Triangles);

			CalculateNeighbors(ref polygons);

			while (ConsolidationStep(polygons)) ;
			Draw(polygons);

			start = new PointWithNeighbors();
			destination = new PointWithNeighbors();

			points = PolygonsToEdgePoints(polygons);
		}

		private List<PointWithNeighbors> PolygonsToEdgePoints(List<PolygonWithNeighbors> polygons)
		{
			var result = new List<PointWithNeighbors>();

			foreach (var poly in polygons)
				foreach (var edge in Edges(poly))
				{
					var point1 = new System.Windows.Point(edge.X1, edge.Y1);
					var point2 = new System.Windows.Point(edge.X2, edge.Y2);
					
					var npoint1 = new PointWithNeighbors();
					npoint1.point=point1;
					var npoint2 = new PointWithNeighbors();
					npoint2.point=point2;

					// simple small edge avoidance
					//if (Distance(npoint1, npoint2) < 20)
					//	continue;

					var center = Center(edge);
					if (result.Any(x => x.point.X == center.point.X && x.point.Y == center.point.Y))
					{
						poly.edgepoints.Add(result.Find(x => x.point.X == center.point.X && x.point.Y == center.point.Y));
					}
					else
					{
						poly.edgepoints.Add(center);
						result.Add(center);
					}
				}

			foreach (var poly in polygons)
			{
				foreach (var point in poly.edgepoints)
				{
					point.neighbors = point.neighbors.Concat(poly.edgepoints).ToList(); ;
					point.neighbors.Remove(point);

					var found = false;

					foreach (var neighbor in poly.neighbors)
					{
						if (neighbor.edgepoints.Any(x => x.point.X == point.point.X && x.point.Y == point.point.Y))
						{
							point.neighbors = point.neighbors.Concat(neighbor.edgepoints).ToList();
							point.neighbors.Remove(point);
							found = true;
						}
					}

					if (!found)
					{
						result.Remove(point);
					}
				}
			}

			foreach (var poly in polygons)
				poly.edgepoints.RemoveAll(x => !result.Contains(x));

			foreach (var point in result)
			{
				point.neighbors.RemoveAll(new Predicate<PointWithNeighbors>(x => !result.Contains(x)));
				point.neighbors.RemoveAll(new Predicate<PointWithNeighbors>(x => x.point.X == point.point.X && x.point.Y == point.point.Y));
				point.neighbors = point.neighbors.GroupBy(x => new Tuple<double, double>(x.point.X, x.point.Y)).Select(y => y.First()).ToList();
			}

			return result;
		}

		private PointWithNeighbors Center(Line edge)
		{
			var point = new PointWithNeighbors();
			point.point.X = (edge.X1 + edge.X2) / 2;
			point.point.Y = (edge.Y1 + edge.Y2) / 2;
			return point;
		}

		private List<Line> Edges(PolygonWithNeighbors poly)
		{
			var result = new List<Line>();

			for (int i = 0; i < poly.polygon.Points.Count; i++)
			{
				var line = new Line();
				var c = poly.polygon.Points.Count;
				line.X1 = poly.polygon.Points[i].X;
				line.Y1 = poly.polygon.Points[i].Y;
				line.X2 = poly.polygon.Points[mod(i + 1, c)].X;
				line.Y2 = poly.polygon.Points[mod(i + 1, c)].Y;
				result.Add(line);
			}

			return result;
		}

		private void Draw(List<PolygonWithNeighbors> polygons)
		{
			canvas.Children.Clear();

			foreach (PolygonWithNeighbors poly in polygons)
			{
				poly.polygon.Stroke = System.Windows.Media.Brushes.Black;
				poly.polygon.Fill = System.Windows.Media.Brushes.White; // to make poly clickable. weird shit.
				canvas.Children.Add(poly.polygon);
			}
		}

		private void CalculateNeighbors(ref List<PolygonWithNeighbors> polygons)
		{
			foreach (PolygonWithNeighbors poly in polygons)
				foreach (PolygonWithNeighbors poly2 in polygons)
					if (Neighbors(poly, poly2) && !poly.neighbors.Contains(poly2) && !poly2.neighbors.Contains(poly))
					{
						poly.neighbors.Add(poly2);
						poly2.neighbors.Add(poly);
					}
		}

		private static bool Neighbors(PolygonWithNeighbors poly, PolygonWithNeighbors poly2)
		{
			if (poly == poly2)
				return false;

			int sharedVertices = 0;
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 3; j++)
					if (poly.polygon.Points[i].X == poly2.polygon.Points[j].X && poly.polygon.Points[i].Y == poly2.polygon.Points[j].Y)
						if (++sharedVertices > 1)
							return true;
			return false;
		}

		private List<PolygonWithNeighbors> ConvertToPolygons(ICollection<Triangle> triangles)
		{
			var polygons = new List<PolygonWithNeighbors>();

			foreach (Triangle triangle in triangles)
			{
				var poly = new PolygonWithNeighbors();
				for (int i = 0; i < 3; i++)
					poly.polygon.Points.Add(new System.Windows.Point(triangle.GetVertex(i).X, triangle.GetVertex(i).Y));
				polygons.Add(poly);
			}

			return polygons;
		}

		private void canvas_MouseMove(object sender, MouseEventArgs e)
		{

			foreach (PolygonWithNeighbors poly in polygons)
			{
				poly.polygon.Fill = System.Windows.Media.Brushes.White;
				poly.polygon.Stroke = System.Windows.Media.Brushes.Black;
				poly.polygon.StrokeThickness = 1;
			}

			foreach (PolygonWithNeighbors poly in polygons)
				if (poly.polygon.IsMouseOver)
				{
					if (start.neighbors.Count != 0)
					{
						foreach (var neighbor in destination.neighbors)
							neighbor.neighbors.Remove(destination);

						destination = new PointWithNeighbors();
						destination.point = e.GetPosition(canvas);
						destination.neighbors = poly.edgepoints;

						foreach (var neighbor in destination.neighbors)
							neighbor.neighbors.Add(destination);

						foreach (var line in pathlines)
							canvas.Children.Remove(line);

						path = WorldProcessing.Planning.Searching.AStarSearch.FindPath(start, destination, Distance, a => 1).ToList();
						pathlines.Clear();

						for (int i = 0; i < path.Count() - 1; i++)
						{
							var line = new Line();
							line.X1 = path[i].point.X;
							line.Y1 = path[i].point.Y;
							line.X2 = path[i + 1].point.X;
							line.Y2 = path[i + 1].point.Y;
							line.Stroke = System.Windows.Media.Brushes.OrangeRed;
							pathlines.Add(line);
						}

						foreach (var line in pathlines)
							canvas.Children.Add(line);
					}

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
							case 2: neighbor.polygon.Fill = System.Windows.Media.Brushes.PaleVioletRed; break;
							case 3: neighbor.polygon.Fill = System.Windows.Media.Brushes.LightPink; break;
							case 4: neighbor.polygon.Fill = System.Windows.Media.Brushes.LightCyan; break;
						}

						text.Text += "   color: ";
						switch (n)
						{
							case 0: text.Text += "Blue\n"; break;
							case 1: text.Text += "Green\n"; break;
							case 2: text.Text += "Red\n"; break;
							case 3: text.Text += "Pink\n"; break;
							case 4: text.Text += "Cyan\n"; break;
						}

						PolygonWithNeighbors hypothetical = Consolidate(poly, neighbor);
						//hypothetical.polygon.Fill = System.Windows.Media.Brushes.OrangeRed;
						//canvas.Children.Add(hypothetical.polygon);

						text.Text += "   convex: " + IsConvex(hypothetical) + "\n";
						//text.Text += "   reachable: " + IsReachable(hypothetical) + "\n";

						//hypothetical.polygon.Fill = System.Windows.Media.Brushes.OrangeRed;
						//canvas.Children.Add(hypothetical.polygon);

						//text.Text += "   newneighbors: " + hypothetical.neighbors.Count + "\n";

						//foreach (var newneighbor in hypothetical.neighbors)
						//{
						//	newneighbor.polygon.StrokeThickness = 3;
						//	//newneighbor.polygon.Fill = System.Windows.Media.Brushes.Black;
						//	if (!canvas.Children.Contains(newneighbor.polygon))
						//		canvas.Children.Add(newneighbor.polygon);
						//}

						//text.Text += "   reachable: " + IsReachable(hypothetical) + "\n";
						//text.Text += "   convex: " + IsConvex(hypothetical) + "\n";
						////text.Text += "   consolidable: " + (IsReachable(hypothetical) && IsConvex(hypothetical)) + "\n";

						n++;
					}
				}
		}

		private double Distance(PointWithNeighbors point, PointWithNeighbors point2)
		{
			return Math.Sqrt(Math.Pow(point.point.X - point2.point.X, 2) + Math.Pow(point.point.Y - point2.point.Y, 2));
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

			if (!p1set || !p2set)
				return false;

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

		private bool IsConvex(PolygonWithNeighbors poly)
		{
			int c = poly.polygon.Points.Count;
			int[] signs = new int[c]; // the sign of zcrossproduct (pos or neg) must be the same for all angles in the polygon
			for (int i = 0; i < c; i++)
			{
				System.Windows.Point p0 = poly.polygon.Points[mod((i - 1), c)];
				System.Windows.Point p1 = poly.polygon.Points[i];
				System.Windows.Point p2 = poly.polygon.Points[mod((i + 1), c)];

				signs[i] = Math.Sign(zcrossproduct(p0, p1, p2));
			}

			return signs.All(x => x >= 0) || signs.All(x => x <= 0);
		}

		private double zcrossproduct(System.Windows.Point p0, System.Windows.Point p1, System.Windows.Point p2)
		{
			double dx1 = p1.X - p0.X;
			double dy1 = p1.Y - p0.Y;
			double dx2 = p2.X - p1.X;
			double dy2 = p2.Y - p1.Y;

			return dx1 * dy2 - dy1 * dx2;
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

			result.polygon.Points.Add(new System.Windows.Point(lines.First().X1, lines.First().Y1));

			while (lines.Count() > 1) // skip the last line as its endpoint is the start point of the first line
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
					result.polygon.Points.Add(new System.Windows.Point(next.X1, next.Y1));
				}
				lines.Remove(next);
			}

			result.neighbors = poly1.neighbors.Concat(poly2.neighbors).GroupBy(item => item.GetHashCode()).Select(group => group.First()).ToList();
			result.neighbors.Remove(poly1);
			result.neighbors.Remove(poly2);

			return result;
		}

		private int mod(int x, int m) // % is remainder which doesn't have the same effect as mod for negative x, hence this function
		{
			return (x % m + m) % m;
		}

		private bool LineContain(List<Line> lines, Line line)
		{
			foreach (var line2 in lines)
				if ((line2.X1 == line.X1 && line2.X2 == line.X2 && line2.Y1 == line.Y1 && line2.Y2 == line.Y2)
					|| (line2.X1 == line.X2 && line2.X2 == line.X1 && line2.Y1 == line.Y2 && line2.Y2 == line.Y1))
					return true;
			return false;
		}

		private bool ConsolidationStep(List<PolygonWithNeighbors> polygons)
		{
			polygons.Sort((a, b) => Math.Sign(Size(a) - Size(b)));

			foreach (PolygonWithNeighbors poly in polygons)
			{
				poly.neighbors.Sort((a, b) => Math.Sign(Size(b) - Size(a)));

				foreach (PolygonWithNeighbors neighbor in poly.neighbors)
				{
					PolygonWithNeighbors hypothetical = Consolidate(poly, neighbor);

					if (IsConvex(hypothetical))//&& IsReachable(hypothetical))
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
			}

			return false;
		}

		private double BoundingSize(PolygonWithNeighbors poly)
		{
			var xmin = (from point in poly.polygon.Points select point.X).Min();
			var xmax = (from point in poly.polygon.Points select point.X).Max();
			var ymin = (from point in poly.polygon.Points select point.Y).Min();
			var ymax = (from point in poly.polygon.Points select point.Y).Max();

			return (xmax - xmin) * (ymax - ymin);
		}

		private double Size(PolygonWithNeighbors poly)
		{
			var points = new List<System.Windows.Point>();
			foreach (var point in poly.polygon.Points)
				points.Add(point);

			points.Add(points[0]);
			return Math.Abs(points.Take(points.Count - 1)
			   .Select((p, i) => (points[i + 1].X - p.X) * (points[i + 1].Y + p.Y))
			   .Sum() / 2);
		}

		private void ConsolidateStep(object sender, RoutedEventArgs e)
		{
			ConsolidationStep(polygons);
			Draw(polygons);
		}

		private void ConsolidateAll(object sender, RoutedEventArgs e)
		{
			while (ConsolidationStep(polygons)) ;
			Draw(polygons);
		}

		private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
			start = new PointWithNeighbors();

			start.point = e.GetPosition(canvas);

			foreach (PolygonWithNeighbors poly in polygons)
				if (poly.polygon.IsMouseOver)
					start.neighbors = start.neighbors.Concat(poly.edgepoints).ToList();
		}
	}
}
