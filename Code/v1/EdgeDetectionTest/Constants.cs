using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDetectionTest
{
    // Standard values as described by Marein:
    // red in foto1 = new Bgr(73, 55, 206)
    // green in foto1 = new Bgr(106, 169, 74)
    public class Constants
	{
		public enum Colors { Red, Green };

		static public Tuple<Bgr,double>[] ColorInfo; // (average,threshold)

		static private readonly double thresholdMultiplier = 2.0;

        static Constants()
        {
			ColorInfo = new Tuple<Bgr, double>[Enum.GetNames(typeof(Colors)).Length];
        }

		static public void UpdateColor(Colors color, Bgr[] data)
		{
			if (data.Length != 0)
			{
				var average = Utility.Average(data);
				var threshold = (from a in data select Utility.ColorDistance(average, a)).Max() * thresholdMultiplier;
				ColorInfo[(int)color] = new Tuple<Bgr,double>(average,threshold);
			}
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
