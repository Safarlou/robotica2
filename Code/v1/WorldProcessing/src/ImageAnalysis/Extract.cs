using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace WorldProcessing.ImageAnalysis
{
	public static class Extract
	{
		public static Tuple<Constants.Color, Image<Gray, byte>>[] ColorMasks(Image<Bgr, byte> image)
		{
			return Util.Image.ColorMask(ref image, Constants.CalibratedColors.ToArray());
		}
		public static Contour<System.Drawing.Point> Contours(Image<Gray, byte> image)
		{
			return image.FindContours(
						Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_LINK_RUNS,
						Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, new MemStorage());
		}

		public static List<Seq<System.Drawing.Point>> Shapes(Contour<System.Drawing.Point> contour)
		{
			var result = new List<Seq<System.Drawing.Point>>();

			using (MemStorage storage = new MemStorage())
			{
				for (; contour != null; contour = contour.HNext)
				{
					// get convex hull
					Seq<System.Drawing.Point> current = contour.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);

					// merge proximal points and remove shallow angle points
					Util.Geo.ConsolidatePoints(current);
					
					if (current.Count() > 3 && current.Area > 10) // magic number, needs better solution
						result.Add(current);
				}
			}

			return result;
		}

		public static List<Representation.Object> Objects(List<Seq<System.Drawing.Point>> shapes, Constants.Color color)
		{
			var result = new List<Representation.Object>();

			foreach (var shape in shapes)
			{
				// hack to get stuff on screen
				var obj = new Representation.Obstacle(Representation.ObstacleType.Block,new Representation.Polygon((from point in shape.GetMinAreaRect(new MemStorage()).GetVertices() select new System.Windows.Point(point.X,point.Y)).ToList()));

				result.Add(obj);
			}

			return result;
		}
	}
}
