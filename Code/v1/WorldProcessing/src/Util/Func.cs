using System.Linq;

namespace WorldProcessing.Util
{
	/// <summary>
	/// Miscelaneous functionality
	/// </summary>
	static class Func
	{
		/// <returns>Whether all bools in the given array are true.</returns>
		public static bool all(bool[] bools)
		{
			return bools.Aggregate(true, (a, b) => a && b);
		}
	}
}
