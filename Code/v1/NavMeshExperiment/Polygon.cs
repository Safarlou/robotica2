using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace NavMeshExperiment
{
	class PolygonWithNeighbors : WorldProcessing.Planning.Searching.IHasNeighbours<PolygonWithNeighbors>
	{
		public Polygon polygon;
		public List<PolygonWithNeighbors> neighbors;
		public List<PointWithNeighbors> edgepoints = new List<PointWithNeighbors>();

		public PolygonWithNeighbors()
		{
			polygon = new Polygon();
			neighbors = new List<PolygonWithNeighbors>();
		}

		IEnumerable<PolygonWithNeighbors> WorldProcessing.Planning.Searching.IHasNeighbours<PolygonWithNeighbors>.Neighbours
		{
			get
			{
				return neighbors;
			}
		}
	}
}
