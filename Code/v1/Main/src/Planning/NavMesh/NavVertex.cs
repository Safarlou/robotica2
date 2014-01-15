using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace WorldProcessing.Planning
{
	/// <summary>
	/// A vertex in the navmesh geometry system
	/// </summary>
	public class NavVertex
	{
		private Point _point = new Point(); // I tried to extend System.Windows.Point but it's sealed so I opted for this route
		public double X { get { return _point.X; } set { _point.X = value; } }
		public double Y { get { return _point.Y; } set { _point.Y = value; } }

		public List<NavVertex> Vertices = new List<NavVertex>();

		/// <summary>
		/// All the edges that this vertex is a member of.
		/// </summary>
		public List<NavEdge> Edges = new List<NavEdge>();
		   
		/// <summary>
		/// All the polygons that this vertex is a member of.
		/// This information is generated from the list of edges that this vertex is a member of.
		/// </summary>
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

		public override string ToString()
		{
			return "{" + X + "," + Y + "}";
		}

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
