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
		public static Tuple<Constants.Colors, Image<Gray, byte>>[] ColorMasks(Image<Bgr, byte> image)
		{
			return Utility.FastColorMask(ref image, (Constants.Colors[])Enum.GetValues(typeof(Constants.Colors)));
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
					Seq<System.Drawing.Point> current = contour.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);

					ConsolidatePoints(current);

					if (current.Count() > 3 && current.Area > 10) // magic number, needs better solution
						result.Add(current);
				}
			}
			return result;
		}

		public static void ConsolidatePoints(Seq<System.Drawing.Point> points)
		{
			var newpoints = points.ToList();

			while (ConsolidatePointsStep(newpoints)) ;

			points.Clear();
			points.PushMulti(newpoints.ToArray(), Emgu.CV.CvEnum.BACK_OR_FRONT.BACK);
		}

		public static bool ConsolidatePointsStep(List<System.Drawing.Point> points)
		{
			var c = points.Count;
			for (int i = 0; i < c; i++)
			{
				var pa = points[i];
				var pb = points[mod(i + 1, c)];

				if (pa != pb && Distance(pa, pb) < 10) // magic number, needs better solution
				{
					points.Insert(i, new System.Drawing.Point((pa.X + pb.X) / 2, (pa.Y + pb.Y) / 2));
					points.Remove(pa);
					points.Remove(pb);
					return true;
				}

				var pz = points[mod(i - 1, c)];

				if (Math.Abs(Angle(pz, pa, pb)) / Math.PI * 180 > 135) // magic number, may need better solution
				{
					points.Remove(pa);
					return true;
				}
			}

			return false;
		}

		private static double Angle(System.Drawing.Point b, System.Drawing.Point a, System.Drawing.Point c)
		{
			var BAx = b.X - a.X;
			var BAy = b.Y - a.Y;
			var CAx = c.X - a.X;
			var CAy = c.Y - a.Y;

			//var BA= b - a; var CA= c - a;  // vector subtraction, to get vector between points
			var dot=    BAx * CAx + BAy * CAy;
			var pcross= BAx * CAy - BAy * CAx;
			var angle = Math.Atan2(pcross, dot);  // this should be the angle BAC, in radians
			return angle;
		}

		private static int mod(int x, int m) // % is remainder which doesn't have the same effect as mod for negative x, hence this function
		{
			return (x % m + m) % m;
		}

		private static double Distance(System.Drawing.Point point, System.Drawing.Point point2)
		{
			return Math.Sqrt(Math.Pow(point.X - point2.X, 2) + Math.Pow(point.Y - point2.Y, 2));
		}

		public static List<Representation.Object> Objects(List<Seq<System.Drawing.Point>> shapes, Constants.Colors color)
		{
			var result = new List<Representation.Object>();

			foreach (var shape in shapes)
			{
				var obj = new Representation.Obstacle(Representation.ObstacleType.Block,new Representation.Polygon((from point in shape.GetMinAreaRect(new MemStorage()).GetVertices() select new System.Windows.Point(point.X,point.Y)).ToList()));

				result.Add(obj);
			}

			return result;
		}
	}
}
