using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.Representation
{
	public class SmartList
	{
		private List<Tuple<Object, int>> Data;

		public SmartList()
		{
			this.Data = new List<Tuple<Object, int>>();
		}
	}
}
