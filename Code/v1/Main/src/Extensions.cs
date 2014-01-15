using System;
using System.Collections.Generic;
using System.Linq;
using WorldProcessing.Planning;
using WorldProcessing.Representation;

namespace WorldProcessing
{
	// extension methods are pretty fancy but they cause me to lose track of which code is located where. So I rather move these (as regular methods) to Util.Nav or similar. Conversions (such as ToList) can probably stay
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

		public static void AddObject(this TriangleNet.Geometry.InputGeometry geo, Representation.Object obj)
		{
			switch (obj.ObjectType)
			{
				case Constants.ObjectType.Wall:
					var wall = (Representation.Wall)obj;
					geo.AddPolygon(wall.Polygon);
					break;
				case Constants.ObjectType.Block:
					break;
				case Constants.ObjectType.Robot:
					break;
				case Constants.ObjectType.TransportRobot:
					break;
				case Constants.ObjectType.GuardRobot:
					break;
				case Constants.ObjectType.Goal:
					break;
			}
		}

		public static void AddPolygon(this TriangleNet.Geometry.InputGeometry geo, Polygon poly)
		{
			//foreach (var point in poly.Points)
			//	geo.AddPoint(point.X, point.Y);

			//var gc = geo.Points.Count();
			//var pc = poly.Points.Count();
			//for (int i = 0; i < pc; i++)
			//	geo.AddSegment(gc - pc + i, gc - pc + (Util.Maths.Mod(i + 1, pc)));

			var geo2 = new TriangleNet.Geometry.InputGeometry();

			foreach (var point in poly.Points)
				geo2.AddPoint(point.X, point.Y);

			var pc = poly.Points.Count();
			for (int i = 0; i < pc; i++)
				geo2.AddSegment(i, (Util.Maths.Mod(i + 1, pc)));

			var mesh = new TriangleNet.Mesh();
			mesh.Triangulate(geo2);

			foreach (var tri in mesh.Triangles)
			{
				geo.AddPoint(tri.GetVertex(0).X, tri.GetVertex(0).Y);
				geo.AddPoint(tri.GetVertex(1).X, tri.GetVertex(1).Y);
				geo.AddPoint(tri.GetVertex(2).X, tri.GetVertex(2).Y);

				var c = geo.Points.Count();

				geo.AddSegment(c - 3, c - 2);
				geo.AddSegment(c - 2, c - 1);
				geo.AddSegment(c - 1, c - 3);

				var pol = new Polygon(new List<System.Windows.Point> { tri.GetVertex(0).ToPoint(), tri.GetVertex(1).ToPoint(), tri.GetVertex(2).ToPoint() });

				geo.AddHole(pol.Centroid.X, pol.Centroid.Y);
			}
		}

		public static List<NavPolygon> ToPolygonList(this TriangleNet.Mesh mesh)
		{
			var vertices = (from vertice in mesh.Vertices select new NavVertex(vertice)).ToList();
			var edges = (from edge in mesh.Edges select new NavEdge(vertices[edge.P0], vertices[edge.P1])).ToList();
			var polygons = (from triangle in mesh.Triangles select new NavPolygon(vertices[triangle.P0], vertices[triangle.P1], vertices[triangle.P2])).ToList();

			return polygons;
		}

		public static List<System.Windows.Point> ToPointList(this TriangleNet.Data.Triangle triangle)
		{
			return new List<System.Windows.Point>(new System.Windows.Point[] { triangle.GetVertex(0).ToPoint(), triangle.GetVertex(1).ToPoint(), triangle.GetVertex(2).ToPoint() });
		}

		public static System.Windows.Point ToPoint(this TriangleNet.Data.Vertex point)
		{
			return new System.Windows.Point(point.X, point.Y);
		}

		public static Representation.Polygon toPolygon(this Emgu.CV.Seq<System.Drawing.Point> points)
		{
			return new Representation.Polygon(points);
		}

		public static Representation.Polygon toSquare(this Emgu.CV.Seq<System.Drawing.Point> points)
		{
			if (points.Count() != 4)
				throw new ArgumentException("Unexpected amount of points.");

			var rect = points.GetMinAreaRect();

			if (rect.size.Width > rect.size.Height)
				rect.size.Height = rect.size.Width;
			else
				rect.size.Width = rect.size.Height;

			return new Representation.Polygon(rect.GetVertices());
		}
	}
}
