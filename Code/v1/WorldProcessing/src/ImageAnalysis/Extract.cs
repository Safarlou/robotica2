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
		public static Tuple<Constants.ObjectType, Image<Gray, byte>>[] ColorMasks(Image<Bgr, byte> image)
		{
			return Util.Image.ColorMask(ref image, Constants.CalibratedObjectTypes.ToArray());
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
		public static List<Seq<System.Drawing.Point>> Shapes(Constants.ObjectType objectType, Contour<System.Drawing.Point> contour)
		{
			var result = new List<Seq<System.Drawing.Point>>();

			using (MemStorage storage = new MemStorage())
			{
				for (; contour != null; contour = contour.HNext)
				{
					switch (objectType)
					{
						case Constants.ObjectType.Wall:
							var wall = contour.ApproxPoly(10);
							if (wall.Area > 1000) // because wall is large and black, this ignores shadowy areas
								result.Add(wall);
							break;
						case Constants.ObjectType.Block:
							FindRectangles(contour, result);
							break;
						case Constants.ObjectType.Robot:
							FindRectangles(contour, result);
							break;
						case Constants.ObjectType.TransportRobot:
							FindRectangles(contour, result);
							break;
						case Constants.ObjectType.GuardRobot:
							FindRectangles(contour, result);
							break;
						case Constants.ObjectType.Goal:
							var goal = contour.ApproxPoly(2);
							if (goal.Area > 500)
								result.Add(goal);
							break;
					}
				}
			}

			return result;
		}

		private static void FindRectangles(Contour<System.Drawing.Point> contour, List<Seq<System.Drawing.Point>> result)
		{
			Seq<System.Drawing.Point> hull = contour.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);
			Consolidate(hull);
			if (hull.Count() > 3 && hull.Area > 10) // magic number, needs better solution
				result.Add(hull);
		}

		/// <summary>
		/// Consolidate a set of points that make up a shape, merging proximal points and removing shallow angle points. TODO: find a better place for this method.
		/// </summary>
		/// <param name="points"></param>
		public static void Consolidate(Seq<System.Drawing.Point> points)
		{
			while (ConsolidateStep(ref points)) ;
		}

		/// <summary>
		/// TODO: find a better place for this method
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		private static bool ConsolidateStep(ref Seq<System.Drawing.Point> points)
		{
			var c = points.Count();
			for (int i = 0; i < c; i++)
			{
				var pz = points[Util.Maths.Mod(i - 1, c)];
				var pa = points[i];
				var pb = points[Util.Maths.Mod(i + 1, c)];

				// shallow angle point removal
				if (Math.Abs(Util.Maths.Angle(pz, pa, pb)) / Math.PI * 180 > 135) // TODO magic number, may need better solution
				{
					points.RemoveAt(i);
					return true;
				}

				// proximal points merging
				if (pa != pb && Util.Maths.Distance(pa, pb) < 10) // TODO magic number, needs better solution
				{
					points.Insert(i, new System.Drawing.Point((pa.X + pb.X) / 2, (pa.Y + pb.Y) / 2));
					points.RemoveAt(i);
					points.RemoveAt(Util.Maths.Mod(i + 1, c));
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
		public static List<Representation.Object> Objects(Constants.ObjectType objectType, List<Seq<System.Drawing.Point>> shapes)
		{
			var result = new List<Representation.Object>();

			foreach (var shape in shapes)
			{
				switch (objectType)
				{
					case Constants.ObjectType.Wall:
						var wall = new Representation.Wall(shape.toPolygon());
						result.Add(wall);
						break;
					case Constants.ObjectType.Block:
						var block = new Representation.Block(shape.toSquare());
						result.Add(block);
						break;
					case Constants.ObjectType.Robot:
						var robot = new Representation.Robot(shape.toPolygon().Centroid);
						result.Add(robot);
						break;
					case Constants.ObjectType.TransportRobot:
						var trobot = new Representation.TransportRobot(shape.toPolygon().Centroid);
						result.Add(trobot);
						break;
					case Constants.ObjectType.GuardRobot:
						var grobot = new Representation.GuardRobot(shape.toPolygon().Centroid);
						result.Add(grobot);
						break;
					case Constants.ObjectType.Goal:
						var goal = new Representation.Goal(shape.toPolygon().Centroid);
						result.Add(goal);
						break;
				}
			}

			return result;
		}

		private static void AddAsPoint(Seq<System.Drawing.Point> shape, List<System.Windows.Point> robots)
		{
			var poly = shape.toPolygon();
			var cent = poly.Centroid;
			robots.Add(cent);
		}
	}
}
