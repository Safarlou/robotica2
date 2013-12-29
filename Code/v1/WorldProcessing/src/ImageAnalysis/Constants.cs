using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing
{
	public static class Constants
	{
		public enum Colors { Red, Green };
		static public List<Colors> AllColors { get { return Enum.GetValues(typeof(Colors)).Cast<Colors>().ToList(); } }
		static public List<Colors> CalibratedColors { get { return AllColors.FindAll(new Predicate<Colors>(x => colorsCalibrated[AllColors.IndexOf(x)])).ToList(); } }
		
		static public Tuple<Bgr, double>[] ColorInfo; // (average,threshold)
		static private bool[] colorsCalibrated;
		static public bool ColorsCalibrated { get { return Utility.all(colorsCalibrated); } }

		static private readonly double thresholdMultiplier = 1.0;

		static Constants()
		{
			ColorInfo = new Tuple<Bgr, double>[Enum.GetNames(typeof(Colors)).Length];
			colorsCalibrated = (from name in Enum.GetNames(typeof(Colors)) select false).ToArray();
		}

		static public void UpdateColor(Colors color, Bgr[] data)
		{
			if (data.Length != 0)
			{
				var average = Utility.Average(data);
				var threshold = (from a in data select Utility.ColorDistance(average, a)).Max() * thresholdMultiplier;
				ColorInfo[(int)color] = new Tuple<Bgr, double>(average, threshold);

				colorsCalibrated[(int)color] = true;
			}
		}

		static public void UpdateColor(Colors color, Image<Bgr, byte> image, Image<Gray, byte> mask)
		{
			List<Bgr> datalist = new List<Bgr>();

			var white = new Gray(255);

			for (int y = image.Rows - 1; y >= 0; y--)
				for (int x = image.Cols - 1; x >= 0; x--)
					if (mask[y, x].Equals(white))
						datalist.Add(image[y, x]);

			UpdateColor(color, datalist.ToArray());
		}

		static public Bgr getColor(Colors color)
		{
			return ColorInfo[(int)color].Item1;
		}

		static public double getThreshold(Colors color)
		{
			return ColorInfo[(int)color].Item2;
		}
	}
}
