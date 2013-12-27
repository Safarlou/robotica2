using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public static List<MCvBox2D> Shapes(Contour<System.Drawing.Point> contour)
		{
			return Utility.FindRectangles(contour);
		}

		public static void Objects(object args)
		{

		}
	}
}
