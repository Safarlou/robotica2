using Emgu.CV.Structure;
using System;
using System.Linq;

namespace WorldProcessing.Util
{
	// methods purely involving color
	static class Color
	{
		public static double EuclideanDistance(Bgr a, Bgr b)
		{
			return Math.Sqrt(Math.Pow(Math.Abs(a.Blue - b.Blue), 2) +
				Math.Pow(Math.Abs(a.Green - b.Green), 2) + Math.Pow(Math.Abs(a.Red - b.Red), 2));
		}

		public static double ComponentDistance(Bgr a, Bgr b)
		{
			return Math.Abs(a.Blue - b.Blue) + Math.Abs(a.Green - b.Green) + Math.Abs(a.Red - b.Red);
		}

		// use ColorDistance throughout, so we can easily switch between implementations
		public static double Distance(Bgr a, Bgr b)
		{
			return ComponentDistance(a, b); // need to use this in order for FastColorExtract to work as currently implemented
			//return EuclideanDistance(a, b); // could work with FastColorExtract if we take out the sqrt, a common optimization (also requires changes to FastColorExtract)
		}

		public static Bgr Average(params Bgr[] args)
		{
			double blue = (from a in args select a.Blue).Sum() / args.Length;
			double green = (from a in args select a.Green).Sum() / args.Length;
			double red = (from a in args select a.Red).Sum() / args.Length;
			return new Bgr(blue, green, red);
		}
	}
}
