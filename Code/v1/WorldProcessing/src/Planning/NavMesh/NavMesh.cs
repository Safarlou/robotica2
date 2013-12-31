using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Geometry;

namespace WorldProcessing.Planning
{
	class NavMesh
	{
		public static List<PolygonWithNeighbours> meshdisplayhack;

		public static List<PolygonWithNeighbours> Generate(List<Representation.Obstacle> objects)
		{
			InputGeometry geo = new InputGeometry();
			geo.AddBounds();
			foreach (var obj in objects) {geo.AddPolygon(obj.Polygon); };

			Mesh mesh = new Mesh();
			//mesh.Behavior.Quality = true;
			//mesh.Behavior.MinAngle = 5;			// todo: tweak number? larger numbers create more triangles, sometimes good, sometimes bad... 5 seems good. in any case we don't want it to create any new points in open space. update: in the real app, it's not working so well.
			mesh.Triangulate(geo);

			var polygons = mesh.ToPolygonList();
			Util.Geo.CalculateNeighbors(ref polygons);

			meshdisplayhack = polygons;

			Util.Geo.Consolidate(polygons);

			return polygons;
		}
	}
}
