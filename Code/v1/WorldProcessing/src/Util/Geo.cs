using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.Util
{
	// any utility methods that deal with geometry, especially when changes are made to the geometry
	static class Geo
	{
		public static void ConsolidatePoints(Seq<System.Drawing.Point> points)
		{
			var newpoints = points.ToList();

			while (ConsolidatePointsStep(newpoints)) ;

			points.Clear();
			points.PushMulti(newpoints.ToArray(), Emgu.CV.CvEnum.BACK_OR_FRONT.BACK);
		}

		private static bool ConsolidatePointsStep(List<System.Drawing.Point> points)
		{
			var c = points.Count;
			for (int i = 0; i < c; i++)
			{
				var pa = points[i];
				var pb = points[Util.Maths.Mod(i + 1, c)];

				// proximal points merging
				if (pa != pb && Util.Maths.Distance(pa, pb) < 10) // TODO magic number, needs better solution
				{
					points.Insert(i, new System.Drawing.Point((pa.X + pb.X) / 2, (pa.Y + pb.Y) / 2));
					points.Remove(pa);
					points.Remove(pb);
					return true;
				}

				var pz = points[Util.Maths.Mod(i - 1, c)];

				// shallow angle point removal
				if (Math.Abs(Util.Maths.Angle(pz, pa, pb)) / Math.PI * 180 > 135) // TODO magic number, may need better solution
				{
					points.Remove(pa);
					return true;
				}
			}

			return false;
		}
	}
}
