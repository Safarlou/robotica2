using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldProcessing.Planning;
using WorldProcessing.Representation;

namespace WorldProcessing.Util
{
	// any utility methods that deal with geometry
	static class Geo
	{
		public static void Consolidate(List<PolygonWithNeighbours> polygons)
		{
			while (ConsolidationStep(polygons)) ;
		}

		private static bool ConsolidationStep(List<PolygonWithNeighbours> polygons)
		{
			polygons.Sort((a, b) => Math.Sign(a.Size - b.Size));

			foreach (PolygonWithNeighbours poly in polygons)
			{
				poly.Neighbours.Sort((a, b) => Math.Sign(b.Size - a.Size));

				foreach (PolygonWithNeighbours neighbor in poly.Neighbours)
				{
					PolygonWithNeighbours hypothetical = Util.Geo.Consolidate(poly, neighbor);

					if (hypothetical.IsConvex)
					{
						polygons.Remove(poly);
						polygons.Remove(neighbor);
						polygons.Add(hypothetical);
						foreach (PolygonWithNeighbours newneighbor in hypothetical.Neighbours)
						{
							newneighbor.Neighbours.Remove(poly);
							newneighbor.Neighbours.Remove(neighbor);
							newneighbor.Neighbours.Add(hypothetical);
						}

						return true; // break out because the iterator has become invalid: restart iteration
					}
				}
			}

			return false;
		}

		// TODO: this function is entirely too large and maybe overly complicated
		public static PolygonWithNeighbours Consolidate(PolygonWithNeighbours poly1, PolygonWithNeighbours poly2)
		{
			PolygonWithNeighbours result = new PolygonWithNeighbours();

			var edges = new List<Edge>();
			foreach (var line in poly1.Edges.Concat(poly2.Edges))
				if (!EdgeContain(poly1.Edges, line) || !EdgeContain(poly2.Edges, line))
					edges.Add(line);

			result.Points.Add(new System.Windows.Point(edges.First().p1.X, edges.First().p1.Y));

			while (edges.Count() > 1) // skip the last line as its endpoint is the start point of the first line
			{
				var prev = result.Points.Last();
				var next = edges.Find(new Predicate<Edge>(a => a.p1.X == prev.X && a.p1.Y == prev.Y));
				if (next != null)
				{
					result.Points.Add(new System.Windows.Point(next.p2.X, next.p2.Y));
				}
				else
				{
					next = edges.Find(new Predicate<Edge>(x => x.p2.X == prev.X && x.p2.Y == prev.Y));
					result.Points.Add(new System.Windows.Point(next.p1.X, next.p1.Y));
				}
				edges.Remove(next);
			}

			result.Neighbours = poly1.Neighbours.Concat(poly2.Neighbours).GroupBy(item => item.GetHashCode()).Select(group => group.First()).ToList();
			result.Neighbours.Remove(poly1);
			result.Neighbours.Remove(poly2);

			return result;
		}

		public static void Consolidate(Seq<System.Drawing.Point> points)
		{
			var newpoints = points.ToList();

			while (ConsolidateStep(newpoints)) ;

			points.Clear();
			points.PushMulti(newpoints.ToArray(), Emgu.CV.CvEnum.BACK_OR_FRONT.BACK);
		}

		private static bool ConsolidateStep(List<System.Drawing.Point> points)
		{
			var c = points.Count;
			for (int i = 0; i < c; i++)
			{
				var pa = points[i];
				var pb = points[Util.Maths.Mod(i + 1, c)];

				// proximal points merging
				if (pa != pb && Util.Maths.Distance(pa, pb) < 10) // TODO magic number, needs better solution
				{
					points.Insert(i, new System.Drawing.Point((pa.X + pb.X) / 2, (pa.Y + pb.Y) / 2));
					points.Remove(pa);
					points.Remove(pb);
					return true;
				}

				var pz = points[Util.Maths.Mod(i - 1, c)];

				// shallow angle point removal
				if (Math.Abs(Util.Maths.Angle(pz, pa, pb)) / Math.PI * 180 > 135) // TODO magic number, may need better solution
				{
					points.Remove(pa);
					return true;
				}
			}

			return false;
		}

		public static void CalculateNeighbors(ref List<PolygonWithNeighbours> polygons)
		{
			foreach (PolygonWithNeighbours poly in polygons)
				foreach (PolygonWithNeighbours poly2 in polygons)
					if (poly != null && poly2 != null && Neighbors(poly, poly2) && !poly.Neighbours.Contains(poly2) && !poly2.Neighbours.Contains(poly))
					{
						poly.Neighbours.Add(poly2);
						poly2.Neighbours.Add(poly);
					}
		}

		public static bool Neighbors(PolygonWithNeighbours poly, PolygonWithNeighbours poly2)
		{
			if (poly == poly2)
				return false;

			int sharedVertices = 0;
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 3; j++)
					if (poly.Points[i].X == poly2.Points[j].X && poly.Points[i].Y == poly2.Points[j].Y)
						if (++sharedVertices > 1)
							return true;
			return false;
		}

		public static bool EdgeContain(List<Edge> lines, Edge line)
		{
			// todo: is component-wise checking necessary or would point-wise checking suffice?
			// TODO: generally, create all the necessary Equals functions so that these kinds of things are unnecessary! (e.g. lines.Contains(line))
			foreach (var line2 in lines)
				if ((line2.p1.X == line.p1.X && line2.p2.X == line.p2.X && line2.p1.Y == line.p1.Y && line2.p2.Y == line.p2.Y)
					|| (line2.p1.X == line.p2.X && line2.p2.X == line.p1.X && line2.p1.Y == line.p2.Y && line2.p2.Y == line.p1.Y))
					return true;
			return false;
		}

		// TODO: ugliest function ever
		public static List<PointWithNeighbours> PolygonsToEdgePoints(List<PolygonWithNeighbours> polygons)
		{
			var result = new List<PointWithNeighbours>();

			// add edgepoints to polygons
			foreach (var poly in polygons)
				foreach (var edge in poly.Edges)
				{
					//// simple small edge avoidance
					//if (Distance(edge.p1, edge.p2) < 20)
					//	continue;
					//// todo: make small area avoidance

					var center = new PointWithNeighbours(edge.Center);

					if (result.Any(a => a.X == center.X && a.Y == center.Y))
					{
						poly.EdgePoints.Add(result.Find(a => a.X == center.X && a.Y == center.Y));
					}
					else
					{
						poly.EdgePoints.Add(center);
						result.Add(center);
					}
				}

			// set edgepoint neighbours
			foreach (var poly in polygons)
				foreach (var point in poly.EdgePoints)
				{
					point.Neighbours.AddRange(point.Neighbours.Concat(poly.EdgePoints).ToList());
					point.Neighbours.Remove(point);

					var found = false;

					foreach (var neighbour in poly.Neighbours)
					{
						if (neighbour.EdgePoints.Any(a => a.X == point.X && a.Y == point.Y))
						{
							point.Neighbours = point.Neighbours.Concat(neighbour.EdgePoints).ToList();
							point.Neighbours.Remove(point);
							found = true;
						}
					}

					if (!found)
					{
						result.Remove(point);
					}
				}

			// remove edgepoints without neighbors (check was done in previous step to remove from result)
			foreach (var poly in polygons)
				poly.EdgePoints.RemoveAll(a => !result.Contains(a));

			// remove self, clones and duplicates from points
			foreach (var point in result)
			{
				point.Neighbours.RemoveAll(new Predicate<PointWithNeighbours>(a => !result.Contains(a)));
				point.Neighbours.RemoveAll(new Predicate<PointWithNeighbours>(a => a.X == point.X && a.Y == point.Y));
				point.Neighbours = point.Neighbours.GroupBy(a => new Tuple<double, double>(a.X, a.Y)).Select(a => a.First()).ToList();
			}

			return result;
		}
	}
}
