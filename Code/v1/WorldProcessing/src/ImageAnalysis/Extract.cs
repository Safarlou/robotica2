using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldProcessing.ImageAnalysis
{
	/// <summary>
	/// Extracting information during analysis
	/// </summary>
	public static class Extract
	{
		/// <summary>
		/// Extract all color masks from an image based on the colors that have been calibrated
		/// </summary>
		/// <param name="image">The image to extract colors from</param>
		/// <returns>An array of (color,mask) tuples</returns>
		public static Tuple<Constants.Color, Image<Gray, byte>>[] ColorMasks(Image<Bgr, byte> image)
		{
			return Util.Image.ColorMask(ref image, Constants.CalibratedColors.ToArray());
		}

		/// <summary>
		/// Extract contours from an image mask
		/// </summary>
		/// <param name="image">Grayscale image presumed to be an image mask (black and white)</param>
		/// <returns>The contour object</returns>
		public static Contour<System.Drawing.Point> Contours(Image<Gray, byte> image)
		{
			return image.FindContours(
						Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_LINK_RUNS,
						Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, new MemStorage());
		}

		/// <summary>
		/// Extract shapes from contours. Essentially cleans the given contours by a set of criteria such that it can be used for object classification.
		/// </summary>
		/// <param name="contour">The contour object.</param>
		/// <returns>A list of shapes.</returns>
		public static List<Seq<System.Drawing.Point>> Shapes(Contour<System.Drawing.Point> contour)
		{
			var result = new List<Seq<System.Drawing.Point>>();

			using (MemStorage storage = new MemStorage())
			{
				for (; contour != null; contour = contour.HNext)
				{
					// get convex hull. maybe not the best solution as any concave object (e.g. angled walls) are corrupted.
					Seq<System.Drawing.Point> current = contour.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);

					// merge proximal points and remove shallow angle points
					Consolidate(current);
					
					if (current.Count() > 3 && current.Area > 10) // magic number, needs better solution
						result.Add(current);
				}
			}

			return result;
		}

		/// <summary>
		/// Consolidate a set of points that make up a shape, merging proximal points and removing shallow angle points. TODO: find a better place for this method.
		/// </summary>
		/// <param name="points"></param>
		public static void Consolidate(Seq<System.Drawing.Point> points)
		{
			var newpoints = points.ToList();

			while (ConsolidateStep(newpoints)) ;

			points.Clear();
			points.PushMulti(newpoints.ToArray(), Emgu.CV.CvEnum.BACK_OR_FRONT.BACK);
		}

		/// <summary>
		/// TODO: find a better place for this method
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		private static bool ConsolidateStep(List<System.Drawing.Point> points)
		{
			var c = points.Count;
			for (int i = 0; i < c; i++)
			{
				var pa = points[i];
				var pb = points[Util.Maths.Mod(i + 1, c)];

				// proximal points merging
				if (pa != pb && Util.Maths.Distance(pa, pb) < 10) // TODO magic number, needs better solution
				{
					points.Insert(i, new System.Drawing.Point((pa.X + pb.X) / 2, (pa.Y + pb.Y) / 2));
					points.Remove(pa);
					points.Remove(pb);
					return true;
				}

				var pz = points[Util.Maths.Mod(i - 1, c)];

				// shallow angle point removal
				if (Math.Abs(Util.Maths.Angle(pz, pa, pb)) / Math.PI * 180 > 135) // TODO magic number, may need better solution
				{
					points.Remove(pa);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Extract objects from shapes, classifying them into <see cref="Representation.Object"/>s.
		/// </summary>
		/// <param name="shapes">A list of shapes.</param>
		/// <param name="color">The color of the mask from which the given shapes were extracted, as color is relevant in object classification</param>
		/// <returns>A list of objects</returns>
		public static List<Representation.Object> Objects(List<Seq<System.Drawing.Point>> shapes, Constants.Color color)
		{
			var result = new List<Representation.Object>();

			foreach (var shape in shapes)
			{
				// hack to get stuff on screen. Everything just gets classified as Obstacles.
				var obj = new Representation.Obstacle(Representation.ObstacleType.Block,new Representation.Polygon((from point in shape.GetMinAreaRect(new MemStorage()).GetVertices() select new System.Windows.Point(point.X,point.Y)).ToList()));

				// when a shape is recognized to be a certain object, it should probably take on the known properties of that object (e.g. a block should become a real square)

				result.Add(obj);
			}

			return result;
		}
	}
}
