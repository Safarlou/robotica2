using System.Collections.Generic;
using System.Windows;
using WorldProcessing.Planning.Searching;

namespace WorldProcessing.Planning
{
	public class PointWithNeighbours : IHasNeighbours<PointWithNeighbours>
	{
		public List<PointWithNeighbours> Neighbours = new List<PointWithNeighbours>();
		
		private Point _point = new Point(); // I tried to extend System.Windows.Point but its properties were unreachable so I opted for this route
		public double X { get { return _point.X; } set { _point.X = value; } }
		public double Y { get { return _point.Y; } set { _point.Y = value; } }

		public PointWithNeighbours(double x, double y)
		{
			_point = new Point(x, y);
		}

		public PointWithNeighbours(Point point)
		{
			_point = point;
		}

		public Point ToPoint()
		{
			return new Point(X, Y);
		}

		IEnumerable<PointWithNeighbours> IHasNeighbours<PointWithNeighbours>.Neighbours
		{
			get { return Neighbours; }
		}
	}
}
