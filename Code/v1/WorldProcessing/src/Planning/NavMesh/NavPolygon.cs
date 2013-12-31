using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System;

namespace WorldProcessing.Planning
{
	public class NavPolygon
	{
		public List<NavVertex> Vertices
		{
			get
			{
				var vertices = new List<NavVertex>();
				var edges = new List<NavEdge>(Edges);

				vertices.Add(edges.First().V0);

				while (edges.Count() > 1) // skip the last line as its endpoint is the start point of the first line
				{
					NavVertex nextvertex;
					var prev = vertices.Last();
					var nextedge = edges.Find(new Predicate<NavEdge>(a => a.V0.X == prev.X && a.V0.Y == prev.Y));
					if (nextedge != null)
						nextvertex = nextedge.V1;
					else
					{
						nextedge = edges.Find(new Predicate<NavEdge>(a => a.V1.X == prev.X && a.V1.Y == prev.Y));
						nextvertex = nextedge.V0;
					}
					edges.Remove(nextedge);
					vertices.Add(nextvertex);
				}

				return vertices;
			}
		}

		public List<NavEdge> Edges = new List<NavEdge>();

		public List<NavPolygon> Polygons
		{
			get
			{
				var result = Edges.Aggregate(new List<NavPolygon>(), (a, b) => a.Union(b.Polygons).ToList());
				result.Remove(this);
				return result;
			}
		}

		public NavPolygon()
		{ }

		public NavPolygon(List<NavEdge> edges, bool relations = true)
		{
			Edges = edges;

			if (relations) // want to be able to not do this when consolidating on shape
				SetRelations();
		}

		public void SetRelations()
		{
			foreach (var edge in Edges)
				edge.Polygons.Add(this);

		}

		public NavPolygon(NavVertex v0, NavVertex v1, NavVertex v2)
			: this(new List<NavEdge> { Util.Nav.SharedEdge(v0, v1), Util.Nav.SharedEdge(v1, v2), Util.Nav.SharedEdge(v2, v0) })
		{ }

		public double Size
		{
			get
			{
				var vertices = new List<NavVertex>(Vertices);

				vertices.Add(vertices[0]);
				return Math.Abs(vertices.Take(vertices.Count - 1)
				   .Select((p, i) => (vertices[i + 1].X - p.X) * (vertices[i + 1].Y + p.Y))
				   .Sum() / 2);
			}
		}

		public bool IsConvex
		{
			get
			{
				int c = Vertices.Count;
				int[] signs = new int[c]; // the sign of zcrossproduct (pos or neg) must be the same for all angles in the polygon
				for (int i = 0; i < c; i++)
				{
					NavVertex p0 = Vertices[Util.Maths.Mod((i - 1), c)];
					NavVertex p1 = Vertices[i];
					NavVertex p2 = Vertices[Util.Maths.Mod((i + 1), c)];

					signs[i] = Math.Sign(Util.Maths.Zcrossproduct(p0, p1, p2));
				}

				return signs.All(x => x >= 0) || signs.All(x => x <= 0);
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			var o = obj as NavPolygon;

			if ((System.Object)o == null)
				return false;

			return Equals(o);
		}

		public bool Equals(NavPolygon o)
		{
			if (o == null)
				return false;

			if (o.Vertices.Count != this.Vertices.Count)
				return false;

			for (int i = 0; i < o.Vertices.Count; i++)
				if (!o.Vertices[i].Equals(this.Vertices[i]))
					return false;

			return true;
		}

		public override int GetHashCode()
		{
			return Vertices.Aggregate(2, (a, b) => a ^ b.GetHashCode());
		}
	}
}
