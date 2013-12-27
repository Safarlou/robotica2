using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.Interface
{
	public static class Draw
	{
		public static Image<Gray, byte> Contours(Image<Bgr,byte> image, Contour<System.Drawing.Point> contours)
		{
			var contoursImage = image.CopyBlank().Convert<Gray, byte>();

			for (Contour<System.Drawing.Point> drawingcontours = contours; drawingcontours != null; drawingcontours = drawingcontours.HNext)
			{
				contoursImage.Draw(drawingcontours, new Gray(256), 1);
			}

			return contoursImage;
		}

		public static Image<Gray, byte> Shapes(Image<Bgr,byte> image, List<MCvBox2D> shapes)
		{
			var shapesImage = image.CopyBlank().Convert<Gray, byte>();

			foreach (MCvBox2D shape in shapes)
				shapesImage.Draw(shape, new Gray(256), 1);

			return shapesImage;
		}

		public static Image<Bgr,byte> Objects(Image<Bgr, byte> image, object objects)
		{
			return image;
		}
	}
}
