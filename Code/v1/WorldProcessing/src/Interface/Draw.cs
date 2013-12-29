using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace WorldProcessing.Interface
{
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
				var hull = shape.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);
				shapesImage.Draw(shape, new Gray(256), 1);
			}

			return shapesImage;
		}

		public static Image<Bgr, byte> Objects(Image<Bgr, byte> image, List<Representation.Object> objects)
		{
			var objectsImage = image.CopyBlank();

			foreach (var obj in objects)
			{
				//var seq = new Seq<System.Drawing.Point>(new MemStorage());
				//seq.PushMulti((from point in ((Representation.Obstacle)obj).Polygon.Points select new System.Drawing.Point((int)point.X, (int)point.Y)).ToArray(), Emgu.CV.CvEnum.BACK_OR_FRONT.BACK);
				var array = (from point in ((Representation.Obstacle)obj).Polygon.Points select new System.Drawing.Point((int)point.X, (int)point.Y)).ToArray();
				objectsImage.DrawPolyline(array, true, new Bgr(0, 0, 255), 1);
			}

			return objectsImage;
		}
	}
}
