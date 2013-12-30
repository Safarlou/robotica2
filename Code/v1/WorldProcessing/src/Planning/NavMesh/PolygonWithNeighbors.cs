using System.Collections.Generic;
using System.Windows;

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
			Neighbours = new List<PolygonWithNeighbours>();
			EdgePoints = new List<PointWithNeighbours>();
		}
	}
}
