using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WorldProcessing.Util
{
	// anything where an image is used
	static class Image
	{
		// for each color, return the mask of that color in the image (using colors and thresholds from Constants)
		static public Tuple<Constants.Color, Image<Gray, byte>>[] ColorMask(ref Image<Bgr, byte> image, Constants.Color[] colors)
		{
			var emptyMask = image.CopyBlank().Convert<Gray, byte>();
			var masks = (from color in colors select new Tuple<Constants.Color, Image<Gray, byte>>(color, emptyMask.Copy())).ToArray(); // a mask for each color

			// get these properties just once instead of repeatedly for each pixel (huge improvement)
			byte[, ,] imageData = image.Data;
			byte[][, ,] masksData = (from m in masks select m.Item2.Data).ToArray();

			// also get these in advance, so we don't have to call methods from Constants for each pixel
			int[][] colorComponents = (from c in colors
									   select new int[] { (int)Constants.getColor(c).Blue, 
                (int)Constants.getColor(c).Green, (int)Constants.getColor(c).Red }).ToArray();
			short[] colorThresholds = (from c in colors select (short)Constants.getThreshold(c)).ToArray();

			byte white = (byte)255; // the masking color
			short diff; // this variable doesn't need to be re-allocated for each pixel

			for (int y = image.Rows - 1; y >= 0; y--) // for each row
				for (int x = image.Cols - 1; x >= 0; x--) // for each column
					for (int c = colors.Length - 1; c >= 0; c--) // for each color
					{
						diff = Util.Maths.Abs(imageData[y, x, 0] - colorComponents[c][0]); // blue difference (using ComponentDistance method)
						if (diff > colorThresholds[c]) // if blue distance too big
							continue; // don't add to mask
						diff += Util.Maths.Abs(imageData[y, x, 1] - colorComponents[c][1]); // blue + green difference
						if (diff > colorThresholds[c]) // if blue + green distance too big
							continue;
						diff += Util.Maths.Abs(imageData[y, x, 2] - colorComponents[c][2]); // blue + green + red difference
						if (diff > colorThresholds[c])
							continue;

						masksData[c][y, x, 0] = white; // add to mask for this color
					}

			return masks;
		}

		public static List<Bgr> PointsToBgr(ref Image<Bgr, byte> im, params System.Drawing.Point[] args)
		{
			List<Bgr> l = new List<Bgr>();
			foreach (System.Drawing.Point p in args)
				l.Add(im[p.Y, p.X]);
			return l;
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

		//public static System.Drawing.Bitmap BitmapSourceToBitmap(BitmapSource srs)
		//{
		//	System.Drawing.Bitmap btm = null;
		//	int width = srs.PixelWidth;
		//	int height = srs.PixelHeight;
		//	int stride = width * ((srs.Format.BitsPerPixel + 7) / 8);
		//	IntPtr ptr = Marshal.AllocHGlobal(height * stride);
		//	srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
		//	btm = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format1bppIndexed, ptr);
		//	return btm;
		//}
	}
}
