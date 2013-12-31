using System.Collections.Generic;
using System.Windows;
using WorldProcessing.Planning.Searching;
using System.Linq;

namespace WorldProcessing.Planning
{
	public class NavVertex //, IHasNeighbours<NavPoint>
	{
		private Point _point = new Point(); // I tried to extend System.Windows.Point but it's sealed so I opted for this route
		public double X { get { return _point.X; } set { _point.X = value; } }
		public double Y { get { return _point.Y; } set { _point.Y = value; } }

		public List<NavVertex> Vertices
		{
			get
			{
				return new List<NavVertex>();
			}
		}

		public List<NavEdge> Edges = new List<NavEdge>();
			   
		public List<NavPolygon> Polygons
		{
			get
			{
				return Edges.Aggregate(new List<NavPolygon>(), (a, b) => a.Union(b.Polygons).ToList());
			}
		}

		public NavVertex(double x, double y)
		{
			_point = new Point(x, y);
		}

		public NavVertex(Point point)
		{
			_point = point;
		}

		public NavVertex(TriangleNet.Data.Vertex vertice)
		{
			_point = vertice.ToPoint();
		}

		public Point ToPoint()
		{
			return new Point(X, Y);
		}

		public System.Drawing.Point ToDrawingPoint()
		{
			return new System.Drawing.Point((int)X, (int)Y);
		}

		//IEnumerable<NavPoint> IHasNeighbours<NavPoint>.Neighbours
		//{
		//	get { return Neighbours; }
		//}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			var o = obj as NavVertex;

			if ((System.Object)o == null)
				return false;

			return Equals(o);
		}

		public bool Equals(NavVertex o)
		{
			if (o == null)
				return false;

			return (o.X == this.X) && (o.Y == this.Y);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}
	}
}
