using System;

namespace WorldProcessing.Util
{
	// called Maths as not to conflict with Math
	static class Maths
	{
		// for use in ColorMask
		public static short Abs(int x)
		{
			return (short)((x ^ (x >> 31)) - (x >> 31));
		}

		public static double Angle(System.Drawing.Point b, System.Drawing.Point a, System.Drawing.Point c)
		{
			var BAx = b.X - a.X;
			var BAy = b.Y - a.Y;
			var CAx = c.X - a.X;
			var CAy = c.Y - a.Y;

			var dot = BAx * CAx + BAy * CAy;
			var pcross = BAx * CAy - BAy * CAx;
			var angle = Math.Atan2(pcross, dot);
			return angle;
		}

		public static int Mod(int x, int m) // % is remainder which doesn't have the same effect as mod for negative x, hence this function
		{
			return (x % m + m) % m;
		}

		public static double Distance(System.Drawing.Point point, System.Drawing.Point point2)
		{
			return Math.Sqrt(Math.Pow(point.X - point2.X, 2) + Math.Pow(point.Y - point2.Y, 2));
		}

		internal static double Zcrossproduct(System.Windows.Point p0, System.Windows.Point p1, System.Windows.Point p2)
		{
			double dx1 = p1.X - p0.X;
			double dy1 = p1.Y - p0.Y;
			double dx2 = p2.X - p1.X;
			double dy2 = p2.Y - p1.Y;

			return dx1 * dy2 - dy1 * dx2;
		}
	}
}
