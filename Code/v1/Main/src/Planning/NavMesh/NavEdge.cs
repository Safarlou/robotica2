﻿using System.Collections.Generic;
using System.Linq;

namespace WorldProcessing.Planning
{
	/// <summary>
	/// An edge in the navmesh geometry system
	/// </summary>
	public class NavEdge
	{
		public NavVertex V0 { get { return Vertices[0]; } }
		public NavVertex V1 { get { return Vertices[1]; } }

		/// <summary>
		/// The vertices that define this edge.
		/// </summary>
		public List<NavVertex> Vertices = new List<NavVertex>();

		/// <summary>
		/// All the edges that are members of polygons that this edge is a member of.
		/// This information is generated from the list of polygons that this edge is a member of.
		/// </summary>
		public List<NavEdge> Edges
		{
			get
			{
				return Polygons.Aggregate(new List<NavEdge>(), (a, b) => a.Union(b.Edges).ToList()).Except(new List<NavEdge>() { this }).ToList();
			}
		}

		/// <summary>
		/// All the polygons that this edge is a member of.
		/// </summary>
		public List<NavPolygon> Polygons = new List<NavPolygon>();

		public NavVertex center;

		public NavEdge(NavVertex p0, NavVertex p1)
		{
			Vertices.Add(p0);
			Vertices.Add(p1);

			p0.Edges.Add(this);
			p1.Edges.Add(this);

			center = new NavVertex((V0.X + V1.X) / 2, (V0.Y + V1.Y) / 2);
			center.Edges.Add(this);
		}

		public double Length { get { return Util.Maths.Distance(V0, V1); } }

		public override string ToString()
		{
			return "[" + V0 + "," + V1 + "]";
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

			return o.V0.Equals(V0) && o.V1.Equals(V1);
		}

		public override int GetHashCode()
		{
			return V0.GetHashCode() ^ V1.GetHashCode();
		}
	}
}
