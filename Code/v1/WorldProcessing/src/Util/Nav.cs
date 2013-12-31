using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldProcessing.Planning;
using WorldProcessing.Representation;

namespace WorldProcessing.Util
{
	// any utility methods that deal with geometry
	static class Nav
	{
		public static void Consolidate(List<NavPolygon> polygons)
		{
			while (ConsolidationStep(polygons)) ;
		}

		private static bool ConsolidationStep(List<NavPolygon> polygons)
		{
			polygons.Sort((a, b) => Math.Sign(a.Size - b.Size)); // consolidate smallest polygon first

			foreach (NavPolygon poly in polygons)
			{
				poly.Polygons.Sort((a, b) => Math.Sign(b.Size - a.Size)); // consolidate with largest neighbour first

				foreach (NavPolygon neighbor in poly.Polygons)
				{
					NavPolygon hypothetical = ConsolidateShape(poly, neighbor);

					if (hypothetical.IsConvex) // this is where more criteria for consolidation can be added
					{
						ConsolidateRelations(hypothetical, poly, neighbor);

						polygons.Remove(poly);
						polygons.Remove(neighbor);
						polygons.Add(hypothetical);

						return true; // break out because the iterator has become invalid: restart iteration
					}
				}
			}

			return false;
		}

		public static NavPolygon ConsolidateShape(NavPolygon poly1, NavPolygon poly2)
		{
			return new NavPolygon(poly1.Edges.Except(poly2.Edges).Union(poly2.Edges.Except(poly1.Edges)).ToList(), false); // false to skip setting relations
		}

		public static void ConsolidateRelations(NavPolygon hypothetical, NavPolygon poly1, NavPolygon poly2)
		{
			hypothetical.SetRelations(); // normally done in construction of NavPolygon, this step was skipped by ConsolidateShape

			foreach (var vertex in poly1.Vertices.Concat(poly2.Vertices))
				vertex.Edges = vertex.Edges.Except(poly1.Edges.Intersect(poly2.Edges)).ToList();

			foreach (var edge in poly1.Edges.Concat(poly2.Edges))
			{
				edge.Polygons = edge.Polygons.Except(new List<NavPolygon> { poly1, poly2 }).ToList();
				edge.Polygons.Add(hypothetical);
			}
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

		//// TODO: ugliest function ever
		//public static List<NavVertex> PolygonsToEdgePoints(List<NavPolygon> polygons)
		//{
		//	var result = new List<NavVertex>();

		//	// add edgepoints to polygons
		//	foreach (var poly in polygons)
		//		foreach (var edge in poly.Edges)
		//		{
		//			//// simple small edge avoidance
		//			//if (Distance(edge.p1, edge.p2) < 20)
		//			//	continue;
		//			//// todo: make small area avoidance

		//			var center = edge.Center;

		//			if (result.Any(a => a.X == center.X && a.Y == center.Y))
		//			{
		//				poly.EdgePoints.Add(result.Find(a => a.X == center.X && a.Y == center.Y));
		//			}
		//			else
		//			{
		//				poly.EdgePoints.Add(center);
		//				result.Add(center);
		//			}
		//		}

		//	// set edgepoint neighbours
		//	foreach (var poly in polygons)
		//		foreach (var point in poly.EdgePoints)
		//		{
		//			point.Neighbours.AddRange(point.Neighbours.Concat(poly.EdgePoints).ToList());
		//			point.Neighbours.Remove(point);

		//			var found = false;

		//			foreach (var neighbour in poly.Neighbours)
		//			{
		//				if (neighbour.EdgePoints.Any(a => a.X == point.X && a.Y == point.Y))
		//				{
		//					point.Neighbours = point.Neighbours.Concat(neighbour.EdgePoints).ToList();
		//					point.Neighbours.Remove(point);
		//					found = true;
		//				}
		//			}

		//			if (!found)
		//			{
		//				result.Remove(point);
		//			}
		//		}

		//	// remove edgepoints without neighbors (check was done in previous step to remove from result)
		//	foreach (var poly in polygons)
		//		poly.EdgePoints.RemoveAll(a => !result.Contains(a));

		//	// remove self, clones and duplicates from points
		//	foreach (var point in result)
		//	{
		//		point.Neighbours.RemoveAll(new Predicate<NavVertex>(a => !result.Contains(a)));
		//		point.Neighbours.RemoveAll(new Predicate<NavVertex>(a => a.X == point.X && a.Y == point.Y));
		//		point.Neighbours = point.Neighbours.GroupBy(a => new Tuple<double, double>(a.X, a.Y)).Select(a => a.First()).ToList();
		//	}

		//	return result;
		//}

		public static NavEdge SharedEdge(NavVertex p1, NavVertex p2)
		{
			return p1.Edges.Intersect(p2.Edges).First();
		}



		public static NavVertex Intersection(NavVertex ps1, NavVertex pe1, NavVertex ps2, NavVertex pe2)
		{
			// Get A,B,C of first line - points : ps1 to pe1
			double A1 = pe1.Y - ps1.Y;
			double B1 = ps1.X - pe1.X;
			double C1 = A1 * ps1.X + B1 * ps1.Y;

			// Get A,B,C of second line - points : ps2 to pe2
			double A2 = pe2.Y - ps2.Y;
			double B2 = ps2.X - pe2.X;
			double C2 = A2 * ps2.X + B2 * ps2.Y;

			// Get delta and check if the lines are parallel
			double delta = A1 * B2 - A2 * B1;
			if (delta == 0)
				throw new System.Exception("Lines are parallel");

			// now return the Vector2 intersection point
			return new NavVertex(
				(B2 * C1 - B1 * C2) / delta,
				(A1 * C2 - A2 * C1) / delta
			);
		}
		public static bool Intersect(NavVertex l1p1, NavVertex l1p2, NavVertex l2p1, NavVertex l2p2)
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

	}
}
