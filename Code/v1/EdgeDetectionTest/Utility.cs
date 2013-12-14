using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
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

        static public void ApplyPerPixel(ref Image<Bgr, byte> im, Func<Bgr, Bgr> func)
        {
            for (int x = 0; x < im.Width; x++)
                for (int y = 0; y < im.Height; y++)
                    im[y, x] = func(im[y, x]);
        }

        static public int EuclideanDistance(Bgr a, Bgr b)
        {
            return (int)Math.Sqrt(Math.Pow(Math.Abs(a.Blue - b.Blue), 2) + 
                Math.Pow(Math.Abs(a.Green - b.Green), 2) + Math.Pow(Math.Abs(a.Red - b.Red), 2));
        }

        static public Bgr MaskByColor(Bgr pixel, Bgr filter, int threshold)
        {
            if (Utility.EuclideanDistance(pixel, filter) < threshold)
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
