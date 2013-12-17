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
            return im.Convert<Gray, byte>().PyrDown().PyrUp().Canny(new Gray(threshold), new Gray(thresholdLinking));
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

		static public Image<Bgr, byte> FastColorExtract(ref Image<Bgr, byte> image)
		{
			var result = image.CopyBlank();

			// get these properties just once instead of repeatedly for each pixel (huge improvement)
			byte[, ,] imageData = image.Data;
			byte[, ,] resultData = result.Data;
			
			// just hacking in red for testing...
			byte red0 = (byte)Constants.Red.Blue;
			byte red1 = (byte)Constants.Red.Green;
			byte red2 = (byte)Constants.Red.Red;
			short redthreshold = (short)Constants.ThresholdRed;

			byte white = (byte)255;
			short diff;

			Stopwatch evaluator = new Stopwatch();
			evaluator.Start();

			for (int y = image.Rows - 1; y >= 0; y--)
				for (int x = image.Cols - 1; x >= 0; x--)
				{
					/*
					if (abshack(imageData[y, x, 0] - red0) + abshack(imageData[y, x, 1] - red1) + abshack(imageData[y, x, 2] - red2) < redthreshold)
						resultData[y, x, 0] = resultData[y, x, 1] = resultData[y, x, 2] = white;
					*/

					// such hack, so speed
					diff = abshack(imageData[y, x, 0] - red0);
					if (diff > redthreshold)
						continue;
					diff += abshack(imageData[y, x, 1] - red1);
					if (diff > redthreshold)
						continue;
					diff += abshack(imageData[y, x, 2] - red2);
					if (diff > redthreshold)
						continue;

					resultData[y, x, 0] = resultData[y, x, 1] = resultData[y, x, 2] = white;
				}
			
			evaluator.Stop();
			Console.WriteLine(evaluator.ElapsedMilliseconds);

			return result;
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
			return ComponentDistance(a, b);
			//return EuclideanDistance(a, b);
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
        static public List<MCvBox2D> FindRectangles(ref Image<Gray, byte> im, double minArea)
        {
            List<MCvBox2D> result = new List<MCvBox2D>();
            //NESTING GO
            using (MemStorage storage = new MemStorage())
            {
                for (Contour<System.Drawing.Point> contours = im.Convert<Gray, byte>().FindContours(
                        Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                        Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, storage); contours != null; contours = contours.HNext)
                {
                    Contour<System.Drawing.Point> current = contours.ApproxPoly(contours.Perimeter * 0.051, storage);
                    if (current.Area >= minArea)
                    {
                        if (current.Total >= 4)
                        {
                            bool isRect = true;
                            System.Drawing.Point[] pts = current.ToArray();
                            LineSegment2D[] edges = Emgu.CV.PointCollection.PolyLine(pts, true);
                            for (int i = 0; i < edges.Length; i++)
                            {
                                double angle = Math.Abs(edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
                                if (angle < 80 && angle > 100)
                                {
                                    isRect = false;
                                    break;
                                }
                            }
                            if (isRect) result.Add(current.GetMinAreaRect());
                        }
                    }
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
