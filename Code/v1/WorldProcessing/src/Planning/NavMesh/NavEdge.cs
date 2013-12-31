using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.Planning
{
	public class NavEdge
	{
		public NavVertex P0 { get { return Vertices[0]; } set { Vertices[0] = value; } }
		public NavVertex P1 { get { return Vertices[1]; } set { Vertices[1] = value; } }

		public List<NavVertex> Vertices = new List<NavVertex>();

		public List<NavEdge> Edges
		{
			get
			{
				return new List<NavEdge>();
			}
		}

		//public override List<NavEdge> Edges
		//{
		//	get
		//	{
		//		var edges = P0.Edges.Concat(P1.Edges).ToList();
		//		edges.Remove(this);
		//		return edges;
		//	}
		//}

		public List<NavPolygon> Polygons = new List<NavPolygon>();

		public NavVertex Center
		{
			get
			{
				return new NavVertex((P0.X + P1.X) / 2, (P0.Y + P1.Y) / 2);
			}
		}

		public NavEdge(NavVertex p0, NavVertex p1)
		{
			Vertices.Add(p0);
			Vertices.Add(p1);

			p0.Edges.Add(this);
			p1.Edges.Add(this);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			var o = obj as NavEdge;

			if ((System.Object)o == null)
				return false;

			return Equals(o);
		}

		public bool Equals(NavEdge o)
		{
			if (o == null)
				return false;

			return o.P0.Equals(P0) && o.P1.Equals(P1);
		}

		public override int GetHashCode()
		{
			return P0.GetHashCode() ^ P1.GetHashCode();
		}
	}
}
