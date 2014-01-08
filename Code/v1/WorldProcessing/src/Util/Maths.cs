using System;
using WorldProcessing.Planning;

namespace WorldProcessing.Util
{
	/// <summary>
	/// Additional Math functionality, called Maths as not to conflict with System.Math.
	/// </summary>
	static class Maths
	{
		/// <summary>
		/// Faster Abs, for use in <see cref="Util.Image.ColorMask"/>.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static short Abs(int x)
		{
			return (short)((x ^ (x >> 31)) - (x >> 31));
		}

		/// <summary>
		/// Angle between vectors a and b from c.
		/// </summary>
		/// <param name="b"></param>
		/// <param name="a"></param>
		/// <param name="c"></param>
		/// <returns></returns>
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

		/// <summary>
		/// World-space angle of the vector from point a to point b
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static double Angle(System.Windows.Point a, System.Windows.Point b)
		{
			return Math.Tan(Math.Abs(a.Y - b.Y) / Math.Abs(a.X - b.X));
		}

		public static double Distance(NavVertex point, NavVertex point2)
		{
			return Math.Sqrt(Math.Pow(point.X - point2.X, 2) + Math.Pow(point.Y - point2.Y, 2));
		}

		/// <summary>
		/// Modulo function. % is remainder which doesn't have the same effect as mod for negative x, hence this function
		/// </summary>
		/// <param name="x"></param>
		/// <param name="m"></param>
		/// <returns></returns>
		public static int Mod(int x, int m)
		{
			return (x % m + m) % m;
		}

		public static double Distance(System.Drawing.Point point, System.Drawing.Point point2)
		{
			return Math.Sqrt(Math.Pow(point.X - point2.X, 2) + Math.Pow(point.Y - point2.Y, 2));
		}

		internal static double Distance(System.Windows.Point point, System.Windows.Point point2)
		{
			return Math.Sqrt(Math.Pow(point.X - point2.X, 2) + Math.Pow(point.Y - point2.Y, 2));
		}

		internal static double Zcrossproduct(NavVertex p0, NavVertex p1, NavVertex p2)
		{
			double dx1 = p1.X - p0.X;
			double dy1 = p1.Y - p0.Y;
			double dx2 = p2.X - p1.X;
			double dy2 = p2.Y - p1.Y;

			return dx1 * dy2 - dy1 * dx2;
		}

		internal static System.Windows.Point Average(System.Windows.Point p, System.Windows.Point q)
		{
			return new System.Windows.Point((p.X + q.X) / 2, (p.Y + q.Y) / 2);
		}
	}
}
