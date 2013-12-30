using System;
using System.Collections.Generic;
using System.Linq;
using WorldProcessing.Planning;
using WorldProcessing.Representation;

namespace WorldProcessing
{
	// extension methods are pretty fancy but they cause me to lose track of which code is located where. So I rather move these (as regular methods) to Util.Geo or NavMesh or similar.
	static class Extensions
	{
		public static void AddBounds(this TriangleNet.Geometry.InputGeometry geo)
		{
			// todo: maybe bounds should not simply be the frame size but something smaller? imageanalysis might need to find bounds of playing field
			geo.AddPoint(0, 0);
			geo.AddPoint(Constants.FrameWidth - 1, 0);
			geo.AddPoint(Constants.FrameWidth - 1, Constants.FrameHeight - 1);
			geo.AddPoint(0, Constants.FrameHeight - 1);

			var c = geo.Points.Count();
			geo.AddSegment(c - 4, c - 3);
			geo.AddSegment(c - 3, c - 2);
			geo.AddSegment(c - 2, c - 1);
			geo.AddSegment(c - 1, c - 4);
		}

		public static void AddPolygon(this TriangleNet.Geometry.InputGeometry geo, Polygon poly)
		{
			foreach (var point in poly.Points)
				geo.AddPoint(point.X, point.Y);

			var gc = geo.Points.Count();
			var pc = poly.Points.Count();
			for (int i = 0; i < pc; i++)
				geo.AddSegment(gc - pc + i, gc - pc + (Util.Maths.Mod(i + 1, pc)));

			var centroid = poly.Centroid;
			geo.AddHole(centroid.X, centroid.Y);
		}

		public static List<PolygonWithNeighbours> ToPolygonList(this TriangleNet.Mesh mesh)
		{
			var polygons = (from triangle in mesh.Triangles select triangle.ToPolygon()).ToList();

			return polygons;
		}

		public static PolygonWithNeighbours ToPolygon(this TriangleNet.Data.Triangle triangle)
		{
			return new PolygonWithNeighbours(triangle.ToPointList());
		}

		public static List<System.Windows.Point> ToPointList(this TriangleNet.Data.Triangle triangle)
		{
			return new List<System.Windows.Point>(new System.Windows.Point[] { triangle.GetVertex(0).ToPoint(), triangle.GetVertex(1).ToPoint(), triangle.GetVertex(2).ToPoint() });
		}

		public static System.Windows.Point ToPoint(this TriangleNet.Data.Vertex point)
		{
			return new System.Windows.Point(point.X, point.Y);
		}
	}
}
