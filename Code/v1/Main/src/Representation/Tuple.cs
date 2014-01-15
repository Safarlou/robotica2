using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.Representation
{
	public class Tuple<T,U>
	{
		public T Item1 { get; set; }
		public U Item2 { get; set; }

		public Tuple() { }

		public Tuple(T item1, U item2)
		{
			this.Item1 = item1;
			this.Item2 = item2;
		}
	}
}
