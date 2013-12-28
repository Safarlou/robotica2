using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace NavMeshExperiment
{
	class PolygonWithNeighbors
	{
		public Polygon polygon;
		public List<PolygonWithNeighbors> neighbors;

		public PolygonWithNeighbors()
		{
			polygon = new Polygon();
			neighbors = new List<PolygonWithNeighbors>();
		}
	}
}
