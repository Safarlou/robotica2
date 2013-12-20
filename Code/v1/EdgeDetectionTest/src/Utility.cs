using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EdgeDetectionTest
{
    class Utility
    {
        static public Image<Gray, byte> FindEdges(ref Image<Bgr, byte> im, double threshold, double thresholdLinking)
        {
            return im.Convert<Gray, byte>().PyrDown().PyrUp().Canny(threshold, thresholdLinking);
        }

		static public Image<TTo, byte> ApplyPerPixel<TFrom, TTo>(ref Image<TFrom, byte> im, Func<TFrom, TTo> func)
			where TFrom : struct, IColor
			where TTo	: struct, IColor
        {
			var result = im.CopyBlank().Convert<TTo, byte>();

			// safe (slow) way
            for (int x = 0; x < im.Width; x++)
                for (int y = 0; y < im.Height; y++)
                    result[y, x] = func(im[y, x]);

			return result;
        }

		// for each color, return the mask of that color in the image (using colors and thresholds from Constants)
		static public Image<Gray, byte>[] FastColorExtract(ref Image<Bgr, byte> image, Constants.Colors[] colors)
		{
            Console.WriteLine(image.Data == null);
			var emptyMask = image.CopyBlank().Convert<Gray, byte>();
			var masks = (from c in colors select emptyMask).ToArray(); // a mask for each color

			// get these properties just once instead of repeatedly for each pixel (huge improvement)
			byte[, ,] imageData = image.Data;
			byte[][, ,] masksData = (from m in masks select m.Data).ToArray();

			// also get these in advance, so we don't have to call methods from Constants for each pixel
			int[][] colorComponents = (from c in colors select new int[] { (int)Constants.getColor(c).Blue, 
                (int)Constants.getColor(c).Green, (int)Constants.getColor(c).Red }).ToArray();
			short[] colorThresholds = (from c in colors select (short)Constants.getThreshold(c)).ToArray();

			byte white = (byte)255; // the masking color
			short diff; // this variable doesn't need to be re-allocated for each pixel

			// performance testing...
			//Stopwatch evaluator = new Stopwatch();
			//evaluator.Start();

			for (int y = image.Rows - 1; y >= 0; y--) // for each row
				for (int x = image.Cols - 1; x >= 0; x--) // for each column
					for (int c = colors.Length - 1; c >= 0; c--) // for each color
					{
						diff = abshack(imageData[y, x, 0] - colorComponents[c][0]); // blue difference (using ComponentDistance method)
						if (diff > colorThresholds[c]) // if blue distance too big
							continue; // don't add to mask
						diff += abshack(imageData[y, x, 1] - colorComponents[c][1]); // blue + green difference
						if (diff > colorThresholds[c]) // if blue + green distance too big
							continue;
						diff += abshack(imageData[y, x, 2] - colorComponents[c][2]); // blue + green + red difference
						if (diff > colorThresholds[c])
							continue;

						masksData[c][y, x, 0] = white; // add to mask for this color
					}
			
			//evaluator.Stop();
			//Console.WriteLine(evaluator.ElapsedMilliseconds);

			return masks;
		}

		static private short abshack(int x)
		{
			return (short)((x ^ (x >> 31)) - (x >> 31));
		}

		static public double EuclideanDistance(Bgr a, Bgr b)
		{
			return Math.Sqrt(Math.Pow(Math.Abs(a.Blue - b.Blue), 2) +
				Math.Pow(Math.Abs(a.Green - b.Green), 2) + Math.Pow(Math.Abs(a.Red - b.Red), 2));
		}

		static public double ComponentDistance(Bgr a, Bgr b)
		{
			return Math.Abs(a.Blue - b.Blue) + Math.Abs(a.Green - b.Green) + Math.Abs(a.Red - b.Red);
		}

		// use ColorDistance throughout, so we can easily switch between implementations
		static public double ColorDistance(Bgr a, Bgr b)
		{
			return ComponentDistance(a, b); // need to use this in order for FastColorExtract to work as currently implemented
			//return EuclideanDistance(a, b); // could work with FastColorExtract if we take out the sqrt, a common optimization (also requires changes to FastColorExtract)
		}

        static public Bgr MaskByColor(Bgr pixel, Bgr filter, double threshold)
        {
			if (Utility.ColorDistance(pixel, filter) < threshold)
                return new Bgr(Color.White);
            else
                return new Bgr(Color.Black);
        }

        static public Bgr Average(params Bgr[] args)
        {
            double blue = (from a in args select a.Blue).Sum() / args.Length;
            double green = (from a in args select a.Green).Sum() / args.Length;
            double red = (from a in args select a.Red).Sum() / args.Length;
            return new Bgr(blue, green, red);
        }

        static public List<Bgr> PointsToBgr(ref Image<Bgr, byte> im, params System.Drawing.Point[] args)
        {
            List<Bgr> l = new List<Bgr>();
			foreach (System.Drawing.Point p in args)
				l.Add(im[p.Y, p.X]);
            return l;
        }

        /// <summary>
        /// Returns -1.0 in case of a failed conversion.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static public double ParseString(string s)
        {
            double result;
            return double.TryParse(s, out result) ? result : -1.0;
        }

        /// <summary>
        /// Finds rectangles in an image
        /// </summary>
        /// <param name="im"></param>
        /// <param name="minArea"></param>
        /// <returns></returns>
		static public List<MCvBox2D> FindRectangles(Contour<System.Drawing.Point> contours)
        {
            List<MCvBox2D> result = new List<MCvBox2D>();
            //NESTING GO
            using (MemStorage storage = new MemStorage())
            {
                for (; contours != null; contours = contours.HNext)
                {
					Seq<System.Drawing.Point> current = contours.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE); // contours.Perimeter * 0.051
					//if (current.Area >= minArea)
					//{
						if (current.Total >= 4)
						{
							bool isRect = true;
							//System.Drawing.Point[] pts = current.ToArray();
							//LineSegment2D[] edges = Emgu.CV.PointCollection.PolyLine(pts, true);
							//for (int i = 0; i < edges.Length; i++)
							//{
							//	double angle = Math.Abs(edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
							//	if (angle < 80 && angle > 100)
							//	{
							//	}
							//}

                            if (isRect) result.Add(current.GetMinAreaRect());
						}
					//}
                }
            }
            return result;
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        static public BitmapSource ToBitmapSource(IImage im)
        {
            using (System.Drawing.Bitmap source = im.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap();
                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }
    }
}
