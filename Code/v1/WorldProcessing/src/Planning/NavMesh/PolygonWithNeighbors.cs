using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WorldProcessing.Planning.Searching;

namespace WorldProcessing.Planning
{
	class PolygonWithNeighbours : Representation.Polygon
	{
		public List<PolygonWithNeighbours> Neighbours { get; set; }
		public List<PointWithNeighbours> EdgePoints { get; set; }

		public PolygonWithNeighbours()
		{
			Neighbours = new List<PolygonWithNeighbours>();
			EdgePoints = new List<PointWithNeighbours>();
		}

		public PolygonWithNeighbours(List<Point> points)
		{
			Points = points;
			EdgePoints = new List<PointWithNeighbours>();
		}
	}
}
