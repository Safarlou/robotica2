using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Geometry;
using WorldProcessing.Planning;

namespace WorldProcessing.src.Planning
{
	/// <summary>
	/// Construct the navigation mesh geometry from a list of objects
	/// </summary>
	public class NavMesh
	{
		public struct NavMeshGenerateResult
		{
			public InputGeometry Geometry { get; private set; }
			public Mesh Trimesh { get; private set; }
			public List<NavPolygon> NavMesh { get; set; }

			public NavMeshGenerateResult(InputGeometry geometry, Mesh trimesh, List<NavPolygon> navMesh) : this()
			{
				Geometry = geometry;
				Trimesh = trimesh;
				NavMesh = navMesh;
			}
		}


		/// <summary>
		/// Construct the navigation mesh geometry from a list of objects
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public static NavMeshGenerateResult Generate(List<Representation.Object> objects)
		{
			InputGeometry geo = new InputGeometry();
			geo.AddBounds(); // adds the world bounds to the geometry
			foreach (var obj in objects) { geo.AddObject(obj); }; // add all the objects

			Mesh mesh = new Mesh();
			mesh.Triangulate(geo); // triangulate the geometry

			var polygons = mesh.ToPolygonList(); // turn it into our own navmesh representation

			Util.Nav.Consolidate(polygons); // turn the triangles into a workable navmesh

			AvoidSmallPassages(ref polygons); // removes unusable connectivity from the mesh;

			return new NavMeshGenerateResult(geo, mesh, polygons);
		}

		/// <summary>
		/// Removes connectivity between mesh edges by looking at 3 consecutive edges on a polygon and checking whether the distance of the projection of either point on the first edge onto the last edge is greater than a certain margin, or vice versa, and if so, removes the connectivity of the middle edge to all other edges (that's to say, the middle edge is said to be too small a passage).
		/// This functionality is currently bugged, removing the connectivity of the wrong edges.
		/// </summary>
		/// <param name="mesh"></param>
		private static void AvoidSmallPassages(ref List<NavPolygon> mesh)
		{
			foreach (var polygon in mesh)
			{
				int c = polygon.Edges.Count;
				for (int i = 0; i < c; i++)
				{
					var e1 = polygon.Edges[i];

					// Polygon.Edges should simply be ordered instead of having to search through it
					var e0 = polygon.Edges.Find(new Predicate<NavEdge>(a => a != e1 && (a.V0 == e1.V0 || a.V1 == e1.V0)));
					var e2 = polygon.Edges.Find(new Predicate<NavEdge>(a => a != e1 && (a.V0 == e1.V1 || a.V1 == e1.V1)));

					if (e1.Polygons.Count < 2 || e0 == null || e2 == null)
						continue;

					// width of vehicle ?
					var M = 20;

					// TODO: Something about this is not yet working correctly. Firstly, the > 0.001 is necessary because sometimes
					// the distance is super tiny (like 10^-13) but the edge involved should actually not be removed.
					// Additionaly, sometimes edges are removed that shouldn't be. All in all, although sometimes promising,
					// this can lead to pretty strange pathing results.
					if (c > 3)
					{
						if ((Util.Maths.Distance(Util.Nav.Project(e1.V0, e2), e1.V0) < M
							&& Util.Maths.Distance(Util.Nav.Project(e1.V0, e2), e1.V0) > 0.001)
							|| (Util.Maths.Distance(Util.Nav.Project(e1.V1, e0), e1.V1) < M
							&& Util.Maths.Distance(Util.Nav.Project(e1.V1, e0), e1.V1) > 0.001))
						{
							//foreach (var poly in e1.Polygons)
							//	poly.Edges.Remove(e1);
							e1.Polygons.Clear();
						}
					}

					// this checks for edges that are simply too short
					if (Util.Maths.Distance(e1.V0, e1.V1) < M)
						//foreach (var poly in e1.Polygons)
						//	poly.Edges.Remove(e1);
						e1.Polygons.Clear();

					c = polygon.Edges.Count;
				}
			}
		}
	}
}
