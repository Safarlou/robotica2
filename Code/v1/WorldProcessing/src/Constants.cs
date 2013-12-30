using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldProcessing
{
	public static class Constants
	{
		public static readonly int FrameWidth = 1600;
		public static readonly int FrameHeight = 1200;

		public enum Color { Red, Green };
		static public List<Color> Colors { get { return Enum.GetValues(typeof(Color)).Cast<Color>().ToList(); } }
		static public List<Color> CalibratedColors { get { return Colors.FindAll(new Predicate<Color>(x => colorsCalibrated[Colors.IndexOf(x)])).ToList(); } }
		
		static public Tuple<Bgr, double>[] ColorInfo; // (average,threshold)
		static private bool[] colorsCalibrated;
		static public bool ColorsCalibrated { get { return Util.Func.all(colorsCalibrated); } }

		static private readonly double thresholdMultiplier = 1.0;

		static Constants()
		{
			ColorInfo = new Tuple<Bgr, double>[Enum.GetNames(typeof(Color)).Length];
			colorsCalibrated = (from name in Enum.GetNames(typeof(Color)) select false).ToArray();
		}

		static public void UpdateColor(Color color, Bgr[] data)
		{
			if (data.Length != 0)
			{
				var average = Util.Color.Average(data);
				var threshold = (from a in data select Util.Color.Distance(average, a)).Max() * thresholdMultiplier;
				ColorInfo[(int)color] = new Tuple<Bgr, double>(average, threshold);

				colorsCalibrated[(int)color] = true;
			}
		}

		static public void UpdateColor(Color color, Image<Bgr, byte> image, Image<Gray, byte> mask)
		{
			List<Bgr> datalist = new List<Bgr>();

			var white = new Gray(255);

			for (int y = image.Rows - 1; y >= 0; y--)
				for (int x = image.Cols - 1; x >= 0; x--)
					if (mask[y, x].Equals(white))
						datalist.Add(image[y, x]);

			UpdateColor(color, datalist.ToArray());
		}

		static public Bgr getColor(Color color)
		{
			return ColorInfo[(int)color].Item1;
		}

		static public double getThreshold(Color color)
		{
			return ColorInfo[(int)color].Item2;
		}
	}
}
