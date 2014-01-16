using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldProcessing.Planning;

namespace WorldProcessing.Interface
{
	/// <summary>
	/// Drawing any results of processing onto images (non-destructively: should create new image)
	/// </summary>
	public static class Draw
	{
		public static Image<Gray, byte> Contours(Image<Bgr, byte> image, Contour<System.Drawing.Point> contours)
		{
			var contoursImage = image.CopyBlank().Convert<Gray, byte>();

			for (Contour<System.Drawing.Point> drawingcontours = contours; drawingcontours != null; drawingcontours = drawingcontours.HNext)
			{
				contoursImage.Draw(drawingcontours, new Gray(256), 1);
			}

			return contoursImage;
		}

		public static Image<Gray, byte> Shapes(Image<Bgr, byte> image, List<Seq<System.Drawing.Point>> shapes)
		{
			var shapesImage = image.CopyBlank().Convert<Gray, byte>();

			foreach (Seq<System.Drawing.Point> shape in shapes)
			{
				//var hull = shape.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE); // drawn as convex hull, doesn't work for concave shapes...
				shapesImage.Draw(shape, new Gray(256), 1);
			}

			return shapesImage;
		}

		public static Image<Bgr, byte> Objects(Image<Bgr, byte> image, List<Representation.Object> objects)
		{
			var objectsImage = new Image<Bgr, byte>(image.Width, image.Height, new Bgr(255, 255, 255));

			foreach (var obj in objects)
			{
				switch (obj.ObjectType)
				{
					case Constants.ObjectType.Wall:
						var array = (from point in ((Representation.Wall)obj).Polygon.Points select new System.Drawing.Point((int)point.X, (int)point.Y)).ToArray();
						objectsImage.DrawPolyline(array, true, Constants.getColor(Constants.ObjectType.Wall), 3);
						break;
					case Constants.ObjectType.Block:
						array = (from point in ((Representation.Block)obj).Polygon.Points select new System.Drawing.Point((int)point.X, (int)point.Y)).ToArray();
						objectsImage.DrawPolyline(array, true, Constants.getColor(Constants.ObjectType.Block), 3);
						break;
					case Constants.ObjectType.Robot:
						// shouldn't exist on its own
						break;
					case Constants.ObjectType.TransportRobot:
						var robot = (Representation.Robot)obj;
						var shape = new System.Drawing.Rectangle((int)obj.Position.X - 40, (int)obj.Position.Y - 40, 40, 40);
						System.Windows.Media.Matrix m = new System.Windows.Media.Matrix();
						m.RotateAt(obj.Orientation / Math.PI * 180, shape.X, shape.Y);

						objectsImage.Draw(shape, Constants.getColor(Constants.ObjectType.TransportRobot), -1);

						var p0 = new System.Drawing.Point((int)robot.Position.X, (int)robot.Position.Y);
						var p1 = new System.Drawing.Point((int)(p0.X + 20 * Math.Cos(robot.Orientation)), (int)(p0.Y + 20 * Math.Sin(robot.Orientation)));
						var line = new LineSegment2D(p0, p1);
						objectsImage.Draw(line, new Bgr(200, 50, 50), 2);
						break;
					case Constants.ObjectType.GuardRobot:
						robot = (Representation.Robot)obj;
						shape = new System.Drawing.Rectangle((int)obj.Position.X - 40, (int)obj.Position.Y - 40, 40, 40);
						m = new System.Windows.Media.Matrix();
						m.RotateAt(obj.Orientation / Math.PI * 180, shape.X, shape.Y);

						objectsImage.Draw(shape, Constants.getColor(Constants.ObjectType.GuardRobot), -1);

						p0 = new System.Drawing.Point((int)robot.Position.X, (int)robot.Position.Y);
						p1 = new System.Drawing.Point((int)(p0.X + 20 * Math.Cos(robot.Orientation)), (int)(p0.Y + 20 * Math.Sin(robot.Orientation)));
						line = new LineSegment2D(p0, p1);
						objectsImage.Draw(line, new Bgr(200, 50, 50), 2);
						break;
					case Constants.ObjectType.Goal:
						var goal = (Representation.Goal)obj;
						shape = new System.Drawing.Rectangle((int)obj.Position.X - 40, (int)obj.Position.Y - 40, 40, 40);

						objectsImage.Draw(shape, Constants.getColor(Constants.ObjectType.Goal), -1);
						break;
				}
			}

			return objectsImage;
		}

		public static Image<Bgr, byte> Geometry(Image<Bgr, byte> image, TriangleNet.Geometry.InputGeometry geo)
		{
			var geoImage = image.Copy();

			foreach (var segment in geo.Segments)
			{
				var p0 = new System.Drawing.Point((int)geo.Points.ToList()[segment.P0].X, (int)geo.Points.ToList()[segment.P0].Y);
				var p1 = new System.Drawing.Point((int)geo.Points.ToList()[segment.P1].X, (int)geo.Points.ToList()[segment.P1].Y);
				var line = new LineSegment2D(p0, p1);
				geoImage.Draw(line, new Bgr(255, 255, 255), 3);
			}

			return geoImage;
		}

		public static IImage Triangles(Image<Bgr, byte> image, TriangleNet.Mesh triangles)
		{
			var triImage = image.Copy();

			foreach (var triangle in triangles.Triangles)
			{
				var p0 = new System.Drawing.Point((int)triangle.GetVertex(0).X, (int)triangle.GetVertex(0).Y);
				var p1 = new System.Drawing.Point((int)triangle.GetVertex(1).X, (int)triangle.GetVertex(1).Y);
				var p2 = new System.Drawing.Point((int)triangle.GetVertex(2).X, (int)triangle.GetVertex(2).Y);
				var line0 = new LineSegment2D(p0, p1);
				var line1 = new LineSegment2D(p1, p2);
				var line2 = new LineSegment2D(p2, p0);
				triImage.Draw(line0, new Bgr(255, 255, 255), 3);
				triImage.Draw(line1, new Bgr(255, 255, 255), 3);
				triImage.Draw(line2, new Bgr(255, 255, 255), 3);
			}

			return triImage;
		}

		public static IImage NavMesh(Image<Bgr, byte> image, List<NavPolygon> navMesh)
		{
			var navMeshImage = image.Copy();

			foreach (var poly in navMesh)
				foreach (var edge in poly.Edges)
				{
					var p0 = new System.Drawing.Point((int)edge.Vertices.First().X, (int)edge.Vertices.First().Y);
					var p1 = new System.Drawing.Point((int)edge.Vertices.Last().X, (int)edge.Vertices.Last().Y);
					var line = new LineSegment2D(p0, p1);
					navMeshImage.Draw(line, new Bgr(255, 255, 255), 3);
				}

			return navMeshImage;
		}

		public static Image<Bgr, byte> Path(Image<Bgr, byte> image, List<NavVertex> path)
		{
			var pathImage = image.Copy();

			for (int i = 0; i < path.Count - 1; i++)
			{
				var l = new LineSegment2D(new System.Drawing.Point((int)path[i].X, (int)path[i].Y), new System.Drawing.Point((int)path[i + 1].X, (int)path[i + 1].Y));
				pathImage.Draw(l, new Bgr(255, 255, 255), 3);
			}

			return pathImage;
		}
	}
}
