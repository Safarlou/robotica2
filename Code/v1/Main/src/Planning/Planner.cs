using System;
using System.Collections.Generic;
using System.Linq;
using WorldProcessing.Representation;

namespace WorldProcessing.Planning
{
	public delegate void PathPlannedEventHandler(object sender, PathPlannedEventArgs e);

	public class PathPlannedEventArgs : EventArgs
	{
		public NavMesh.NavMeshGenerateResult NavMeshResult { get; private set; }
		public List<NavVertex> Connectivity { get; private set; }
		public List<NavVertex> Path { get; private set; }
		public Planning.Actions.Action TransportRobotAction { get; private set; }
		public Planning.Actions.Action GuardRobotAction { get; private set; }

		public PathPlannedEventArgs(NavMesh.NavMeshGenerateResult navMeshResult, List<NavVertex> connectivity, List<NavVertex> path, Planning.Actions.Action transport, Planning.Actions.Action guard)
		{
			NavMeshResult = navMeshResult;
			Connectivity = connectivity;
			Path = path;
			TransportRobotAction = transport;
			GuardRobotAction = guard;
		}
	}

	/// <summary>
	/// The Planner creates a plan for the transport/guard robots,
	/// according to calculations on the WorldModel.
	/// </summary>
	public class Planner : AbstractPlanner
	{
		public event PathPlannedEventHandler PathPlannedEvent = delegate { };

		public Planner(WorldModel model)
			: base(model)
		{
			model.ModelUpdatedEvent += OnModelUpdatedEvent;
		}

		public override void PlanTransport()
		{
			throw new NotImplementedException();
		}

		public override void PlanGuard()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// A bit of a hacked-together function at the moment for testing purposes. Takes the worldmodel and creates a path between two distantiated vertices.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void OnModelUpdatedEvent(object sender, EventArgs args)
		{
			try
			{
				if (Constants.ObjectTypesCalibrated)
				{
					var model = (WorldModel)sender;
					var results = NavMesh.Generate((from obj in ((WorldModel)sender).Walls select (Representation.Object)obj).ToList());

					var start = new NavVertex(model.TransportRobot.Position);
					var end = new NavVertex(model.Goal.Position);

					var startpoly = FindContainingPolygon(results.NavMesh, start);
					var endpoly = FindContainingPolygon(results.NavMesh, end);

					Searching.Path<NavVertex> path;
					List<NavVertex> vertices = null;

					if (startpoly == endpoly)
					{
						path = new Searching.Path<NavVertex>(end);
					}
					else
					{
						InsertIntoMesh(results.NavMesh, start);
						InsertIntoMesh(results.NavMesh, end);

						var edges = ToEdges(results.NavMesh);
						vertices = ToVertices(edges);

						path = WorldProcessing.Planning.Searching.AStarSearch.FindPath(start, end, a => a.Vertices, Util.Maths.Distance, a => 0);
					}

					if (path == null)
					{
						var transportAction = new Actions.WaitAction();
						var guardAction = new Actions.WaitAction();
						PathPlannedEvent(this, new PathPlannedEventArgs(results, vertices, null, transportAction, guardAction));
						return;
					}

					var pathlist = path.ToList();

					while (RefinePath(ref pathlist)) ; // initial path is between edge centers, this fits it around bends more snugly

					// if almost at next node, remove node
					var ReachedNodeMargin = 20;
					while (Util.Maths.Distance(model.TransportRobot.Position, pathlist.First().ToPoint()) < ReachedNodeMargin)
						pathlist.RemoveAt(0);

					var intersection = FindFirstPathIntersection(pathlist, model.Blocks);

					if (intersection == null)
					{
						var transportAction = new Actions.MovementAction(path.First().ToPoint());
						var guardAction = new Actions.WaitAction();
						PathPlannedEvent(this, new PathPlannedEventArgs(results, vertices, pathlist, transportAction, guardAction));
						return;
					}
					else
					{
						var transportAction = new Actions.WaitAction();

						//var results = NavMesh.Generate((from obj in ((WorldModel)sender).Walls select (Representation.Object)obj).ToList());

						var guardAction = new Actions.WaitAction();

						PathPlannedEvent(this, new PathPlannedEventArgs(results, vertices, pathlist, transportAction, guardAction));
						return;
					}
				}
			}
			catch { return; }
		}

		private Block FindFirstPathIntersection(List<NavVertex> path, List<Block> blocks)
		{
			var edges = new List<NavEdge>();

			for (int i = 0; i < path.Count - 1; i++)
			{
				edges.Add(new NavEdge(path[i], path[i + 1]));
			}

			foreach (var edge in edges)
				foreach (var block in blocks)
				{
					var pos = new NavVertex(block.Position.X, block.Position.Y);

					var projection = Util.Nav.Project(pos, edge);

					var distance = Util.Maths.Distance(pos, projection);

					var Margin = 50; // size of robot + block (/2)

					if (distance < Margin)
						return block;
				}

			return null;
		}

		private NavPolygon FindContainingPolygon(List<NavPolygon> mesh, NavVertex vertex)
		{
			foreach (var poly in mesh)
				if (poly.ContainsPoint(vertex))
					return poly;

			return null;
		}

		private void InsertIntoMesh(List<NavPolygon> mesh, NavVertex vertex)
		{
			var poly = FindContainingPolygon(mesh, vertex);

			foreach (var edgevertex in (from edge in poly.Edges select edge.center))
			{
				edgevertex.Vertices.Add(vertex);
				vertex.Vertices.Add(edgevertex);
			}

			return;
		}

		// It's so ugly I wanna die! Not really but it sure needs refactoring.
		public bool RefinePath(ref List<NavVertex> path)
		{
			bool changed = false;

			while (RefinePathStep(ref path)) changed = true;
			// repeat until no points are changed

			if (!changed)
				return false;

			// width of vehicle ?
			var Margin = 50;

			// move vertices to corners
			for (int i = 1; i < path.Count - 1; )
			{
				if (path[i].Edges.Count == 0)
				{
					i++;
					continue;
				}

				var vertex = path[i];
				var edge = path[i].Edges.First();
				var distTo0 = Util.Maths.Distance(vertex, edge.V0);
				var distTo1 = Util.Maths.Distance(vertex, edge.V1);

				if (distTo0 < distTo1)
				{
					if (distTo0 < Margin)
					{
						vertex.X = edge.V0.X;
						vertex.Y = edge.V0.Y;
						i++;
					}
					else
					{
						i++;
						//path.RemoveAt(i);
					}
				}
				else
				{
					if (distTo1 < Margin)
					{
						vertex.X = edge.V1.X;
						vertex.Y = edge.V1.Y;
						i++;
					}
					else
					{
						i++;
						//path.RemoveAt(i);
					}
				}
			}

			// merge proximal points
			var MergeThreshold = 1;

			for (int i = 1; i < path.Count - 1; )
			{
				if (Util.Maths.Distance(path[i], path[i + 1]) < MergeThreshold)
				{
					path.RemoveAt(i + 1);
				}
				else
				{
					i++;
				}
			}

			for (int i = 1; i < path.Count - 1; i++)
			{
				if (path[i].Edges.Count == 0)
					continue;

				var vertex = path[i];
				var onEdge = path[i].Edges.First();
				var distTo0 = Util.Maths.Distance(vertex, onEdge.V0);
				var distTo1 = Util.Maths.Distance(vertex, onEdge.V1);

				if (distTo0 > Margin && distTo1 > Margin)
					continue;

				NavVertex onVertex = null;

				if (distTo0 < distTo1)
					onVertex = onEdge.V0;
				else
					onVertex = onEdge.V1;

				var edges = onVertex.Edges.FindAll(new Predicate<NavEdge>(a => a.Polygons.Count == 1));

				var firstEdgeVert = edges.First().Vertices.Find(new Predicate<NavVertex>(a => a != onVertex));
				var secondEdgeVert = edges.Last().Vertices.Find(new Predicate<NavVertex>(a => a != onVertex));

				var firstdx = (onVertex.X - firstEdgeVert.X) / edges.First().Length;
				var firstdy = (onVertex.Y - firstEdgeVert.Y) / edges.First().Length;
				var seconddx = (onVertex.X - secondEdgeVert.X) / edges.Last().Length;
				var seconddy = (onVertex.Y - secondEdgeVert.Y) / edges.Last().Length;

				var firstdxperp = firstdy;
				var firstdyperp = -firstdx;
				var seconddxperp = firstdy;
				var seconddyperp = -firstdx;

				// sum those

				var sumdx = firstdx + seconddx;
				var sumdy = firstdy + seconddy;

				// add to path[i] * Margin

				path[i].X += sumdx * Margin;
				path[i].Y += sumdy * Margin;

				path[i].Edges.Clear();
			}

			return true;
		}

		/// <summary>
		/// Fit the edge-centered path snugly around the corners it passes around.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool RefinePathStep(ref List<NavVertex> path)
		{
			var changed = false;
			// foreach point0,1,2 on path
			int c = path.Count;
			for (int i = 1; i < c - 1; i++)
			{
				if (path[i].Edges.Count == 0)
					continue;

				var v0 = path[i - 1];
				var v1 = path[i];
				var v2 = path[i + 1];

				// get edge of point1
				var edge = v1.Edges.First();

				// calculate intersection of edge and line(point0,point2)
				var intersection = Util.Nav.Intersection(edge.V0, edge.V1, v0, v2);

				// get endpoint of edge closest to intersection
				var endpoint = Util.Maths.Distance(edge.V0, intersection) < Util.Maths.Distance(edge.V1, intersection) ? edge.V0 : edge.V1;
				var otherpoint = edge.Vertices.Except(new List<NavVertex> { endpoint }).First();

				// calculate point on edge at Margin distance from endpoint
				// http://math.stackexchange.com/questions/134112/find-a-point-on-a-line-segment-located-at-a-distance-d-from-one-endpoint
				var M = 0;
				var dist = Math.Sqrt(Math.Pow(endpoint.X - otherpoint.X, 2) + Math.Pow(endpoint.Y - otherpoint.Y, 2));
				var x = endpoint.X + M * (otherpoint.X - endpoint.X) / dist;
				var y = endpoint.Y + M * (otherpoint.Y - endpoint.Y) / dist;
				var marginpoint = new NavVertex(x, y);

				// of intersection and marginpoint, get the one closest to v1
				var closest = Util.Maths.Distance(intersection, v1) < Util.Maths.Distance(marginpoint, v1) ? intersection : marginpoint;

				//check if intersection lies outside margin
				if (Util.Nav.Intersect(v0, v2, marginpoint, otherpoint))
				{
					closest = intersection;
				}
				else
				{
					closest = marginpoint;
				}

				// set point1 to this point
				var threshold = 0.01; // often the process can continue iterating forever, so the potential change in location needs to be larger than this threshold (scale in pixels)
				if (Math.Abs(closest.X - v1.X) > threshold || Math.Abs(closest.Y - v1.Y) > threshold)
				{
					v1.X = closest.X;
					v1.Y = closest.Y;
					changed = true;
				}
			}

			return changed;
		}

		public List<NavEdge> ToEdges(List<NavPolygon> polygons)
		{
			var result = new List<NavEdge>();

			foreach (var polygon in polygons)
				foreach (var edge in polygon.Edges)
					if (!result.Contains(edge))
						result.Add(edge);

			return result;
		}

		public List<NavVertex> ToVertices(List<NavEdge> edges)
		{
			var result = new List<NavVertex>();

			foreach (var edge in edges)
			{
				result.Add(edge.center);
				foreach (var pol in edge.Polygons)
					foreach (var edge2 in pol.Edges)
						if (edge2 != edge)
							edge.center.Vertices.Add(edge2.center);
			}

			return result;
		}
	}
}
