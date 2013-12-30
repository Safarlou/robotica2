using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Geometry;
using WorldProcessing.Planning;
using System.Linq;

namespace WorldProcessing.Planning.NavMesh
{
	class NavMesh
	{
		public static void Generate(List<Representation.Obstacle> objects)
		{
			InputGeometry geo = new InputGeometry();
			geo.AddBounds();
			foreach (var obj in objects) geo.AddPolygon(obj.Polygon);

			Mesh mesh = new Mesh();
			mesh.Behavior.Quality = true;
			mesh.Behavior.MinAngle = 5;			// todo: tweak number? larger numbers create more triangles, sometimes good, sometimes bad... 5 seems good. in any case we don't want it to create any new points in open space
			mesh.Triangulate(geo);

			var polygons = mesh.ToPolygonList();
			Util.Geo.CalculateNeighbors(ref polygons);

			Consolidate(polygons);
		}

		private static void Consolidate(List<PolygonWithNeighbours> polygons)
		{
			while (ConsolidationStep(polygons)) ;
		}

		private static bool ConsolidationStep(List<PolygonWithNeighbours> polygons)
		{
			polygons.Sort((a, b) => Math.Sign(a.Size - b.Size));

			foreach (PolygonWithNeighbours poly in polygons)
			{
				poly.Neighbours.Sort((a, b) => Math.Sign(b.Size - a.Size));

				foreach (PolygonWithNeighbours neighbor in poly.Neighbours)
				{
					PolygonWithNeighbours hypothetical = Util.Geo.Consolidate(poly, neighbor);

					if (hypothetical.IsConvex)
					{
						polygons.Remove(poly);
						polygons.Remove(neighbor);
						polygons.Add(hypothetical);
						foreach (PolygonWithNeighbours newneighbor in hypothetical.Neighbours)
						{
							newneighbor.Neighbours.Remove(poly);
							newneighbor.Neighbours.Remove(neighbor);
							newneighbor.Neighbours.Add(hypothetical);
						}

						return true; // break out because the iterator has become invalid: restart iteration
					}
				}
			}

			return false;
		}
	}
}
