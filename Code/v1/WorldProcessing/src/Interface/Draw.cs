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
						break;
					case Constants.ObjectType.GuardRobot:
						robot = (Representation.Robot)obj;
						shape = new System.Drawing.Rectangle((int)obj.Position.X - 40, (int)obj.Position.Y - 40, 40, 40);
						m = new System.Windows.Media.Matrix();
						m.RotateAt(obj.Orientation / Math.PI * 180, shape.X, shape.Y);

						objectsImage.Draw(shape, Constants.getColor(Constants.ObjectType.GuardRobot), -1);
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

		public static Image<Bgr, byte> Path(Image<Bgr, byte> image, List<NavVertex> path)
		{
			var pathImage = image.Copy();

			for (int i = 0; i < path.Count - 1; i++)
			{
				var l = new LineSegment2D(new System.Drawing.Point((int)path[i].X, (int)path[i].Y), new System.Drawing.Point((int)path[i + 1].X, (int)path[i + 1].Y));
				pathImage.Draw(l, new Bgr(128, 128, 128), 4);
			}

			return pathImage;
		}
	}
}
