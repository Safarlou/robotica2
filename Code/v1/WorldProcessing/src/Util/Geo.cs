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

		//public static void CalculateNeighbors(ref List<NavPolygon> polygons)
		//{
		//	foreach (NavPolygon poly in polygons)
		//		foreach (NavPolygon poly2 in polygons)
		//			if (poly != null && poly2 != null && Neighbors(poly, poly2) && !poly.Neighbours.Contains(poly2) && !poly2.Neighbours.Contains(poly))
		//			{
		//				poly.Neighbours.Add(poly2);
		//				poly2.Neighbours.Add(poly);
		//			}
		//}

		//public static bool Neighbors(NavPolygon poly, NavPolygon poly2)
		//{
		//	if (poly == poly2)
		//		return false;

		//	int sharedVertices = 0;
		//	for (int i = 0; i < 3; i++)
		//		for (int j = 0; j < 3; j++)
		//			if (poly.Vertices[i].X == poly2.Vertices[j].X && poly.Vertices[i].Y == poly2.Vertices[j].Y)
		//				if (++sharedVertices > 1)
		//					return true;
		//	return false;
		//}

		//public static bool EdgeContain(List<Edge> lines, Edge line)
		//{
		//	// todo: is component-wise checking necessary or would point-wise checking suffice?
		//	// TODO: generally, create all the necessary Equals functions so that these kinds of things are unnecessary! (e.g. lines.Contains(line))
		//	foreach (var line2 in lines)
		//		if ((line2.p1.X == line.p1.X && line2.p2.X == line.p2.X && line2.p1.Y == line.p1.Y && line2.p2.Y == line.p2.Y)
		//			|| (line2.p1.X == line.p2.X && line2.p2.X == line.p1.X && line2.p1.Y == line.p2.Y && line2.p2.Y == line.p1.Y))
		//			return true;
		//	return false;
		//}

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
	}
}
