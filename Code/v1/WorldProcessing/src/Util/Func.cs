using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
