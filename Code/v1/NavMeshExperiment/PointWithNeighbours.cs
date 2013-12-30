using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NavMeshExperiment
{
	class PointWithNeighbors : WorldProcessing.Planning.Searching.IHasNeighbours<PointWithNeighbors>
	{
		public List<PointWithNeighbors> neighbors = new List<PointWithNeighbors>();
		public System.Windows.Point point = new System.Windows.Point();

		IEnumerable<PointWithNeighbors> WorldProcessing.Planning.Searching.IHasNeighbours<PointWithNeighbors>.Neighbours
		{
			get { return neighbors; }
		}
	}
}
