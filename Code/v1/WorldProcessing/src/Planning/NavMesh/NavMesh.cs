using System;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Geometry;

namespace WorldProcessing.Planning
{
	/// <summary>
	/// Construct the navigation mesh geometry from a list of objects
	/// </summary>
	class NavMesh
	{
		public static List<NavPolygon> meshdisplayhack; // used to get it onto the screen, should of course be handled through an event that the interface subscribes to

		/// <summary>
		/// Construct the navigation mesh geometry from a list of objects
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public static List<NavPolygon> Generate(List<Representation.Obstacle> objects)
		{
			InputGeometry geo = new InputGeometry();
			geo.AddBounds(); // adds the world bounds to the geometry
			foreach (var obj in objects) {geo.AddPolygon(obj.Polygon); }; // add all the objects

			Mesh mesh = new Mesh();
			mesh.Triangulate(geo); // triangulate the geometry

			var polygons = mesh.ToPolygonList(); // turn it into our own navmesh representation

			meshdisplayhack = polygons; // display hack

			Util.Nav.Consolidate(polygons); // turn the triangles into a workable navmesh

			return polygons;
		}
	}
}
