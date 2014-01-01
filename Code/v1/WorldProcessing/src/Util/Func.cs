using System.Linq;

namespace WorldProcessing.Util
{
	static class Func
	{
		public static bool all(bool[] bools)
		{
			return bools.Aggregate(true, (a, b) => a && b);
		}
	}
}
