using System;
using System.Collections.Generic;
using System.Linq;
using WorldProcessing.Planning;

namespace WorldProcessing.Util
{
	/// <summary>
	/// Utility functions for Navigation geometry
	/// </summary>
	static class Nav
	{
		/// <summary>
		/// Turn a list of polygons (typically triangles) into polygons suitable for navigation by consolidating them iteratively
		/// </summary>
		/// <param name="polygons"></param>
		public static void Consolidate(List<NavPolygon> polygons)
		{
			while (ConsolidationStep(polygons)) ;
		}

		/// <summary>
		/// Look for a pair of polygons to consolidate and consolidate them
		/// </summary>
		/// <param name="polygons"></param>
		/// <returns>Whether a pair was found to consolidate</returns>
		private static bool ConsolidationStep(List<NavPolygon> polygons)
		{
			polygons.Sort((a, b) => Math.Sign(a.Area - b.Area)); // consolidate smallest polygon first

			foreach (NavPolygon poly in polygons)
			{
				poly.Polygons.Sort((a, b) => Math.Sign(b.Area - a.Area)); // consolidate with largest neighbour first

				foreach (NavPolygon neighbor in poly.Polygons)
				{
					NavPolygon hypothetical = ConsolidateShape(poly, neighbor); // check out the shape of the hypothetical consolidation

					if (hypothetical.IsConvex) // this is where more criteria for consolidation can be added
					{
						ConsolidateRelations(hypothetical, poly, neighbor); // afixate the consolidation by updating the connectivity

						polygons.Remove(poly);
						polygons.Remove(neighbor);
						polygons.Add(hypothetical);

						return true; // break out because the iterator has become invalid: restart iteration
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Calculate the result of consolidating only the shape of two polygons, not changing the connectivity
		/// </summary>
		/// <param name="poly1"></param>
		/// <param name="poly2"></param>
		/// <returns></returns>
		public static NavPolygon ConsolidateShape(NavPolygon poly1, NavPolygon poly2)
		{
			return new NavPolygon(poly1.Edges.Except(poly2.Edges).Union(poly2.Edges.Except(poly1.Edges)).ToList(), false); // false to skip setting relations
		}

		/// <summary>
		/// Consolidate the connectivity (relations to other geometry) two polygons and their hypothetical consolidated result.
		/// </summary>
		/// <param name="hypothetical"></param>
		/// <param name="poly1"></param>
		/// <param name="poly2"></param>
		public static void ConsolidateRelations(NavPolygon hypothetical, NavPolygon poly1, NavPolygon poly2)
		{
			hypothetical.SetRelations(); // normally done in construction of NavPolygon, this step was skipped by ConsolidateShape

			foreach (var vertex in poly1.Vertices.Concat(poly2.Vertices))
				vertex.Edges = vertex.Edges.Except(poly1.Edges.Intersect(poly2.Edges)).ToList();

			foreach (var edge in poly1.Edges.Union(poly2.Edges))
			{
				edge.Polygons = edge.Polygons.Except(new List<NavPolygon> { poly1, poly2 }).ToList();
				if (!edge.Polygons.Contains(hypothetical))
					edge.Polygons.Add(hypothetical);
			}
		}

		/// <summary>
		/// Return the edge that connects two vertices.
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public static NavEdge SharedEdge(NavVertex p1, NavVertex p2)
		{
			return p1.Edges.Intersect(p2.Edges).First();
		}

		/// <summary>
		/// Return a vertex that is at the intersection of two infinite lines defined by two pairs of points.
		/// </summary>
		/// <param name="ps1"></param>
		/// <param name="pe1"></param>
		/// <param name="ps2"></param>
		/// <param name="pe2"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Determine whether two line segments intersect, defined by the endpoints of the line segments.
		/// </summary>
		/// <param name="l1p1"></param>
		/// <param name="l1p2"></param>
		/// <param name="l2p1"></param>
		/// <param name="l2p2"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Project a point onto a line, both defined by navigation geometry.
		/// </summary>
		/// <param name="toProject"></param>
		/// <param name="edge"></param>
		/// <returns></returns>
		public static NavVertex Project(NavVertex toProject, NavEdge edge)
		{
			var line1 = edge.V0;
			var line2 = edge.V1;

			double m = (double)(line2.Y - line1.Y) / (line2.X - line1.X);
			double b = (double)line1.Y - (m * line1.X);

			double x = (m * toProject.Y + toProject.X - m * b) / (m * m + 1);
			double y = (m * m * toProject.Y + m * toProject.X + b) / (m * m + 1);

			return new NavVertex(x, y);
		}
	}
}
