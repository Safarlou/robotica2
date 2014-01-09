using System;
using System.Collections.Generic;
using System.Linq;
using WorldProcessing.Representation;

namespace WorldProcessing.Planning
{
	public delegate void PathPlannedEventHandler(object sender, PathPlannedEventArgs e);

	public class PathPlannedEventArgs : EventArgs
	{
		public Action TransportRobotAction { get; private set; }
		public Action GuardRobotAction { get; private set; }

		public PathPlannedEventArgs(Action transport, Action guard)
		{
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
			//model.ModelUpdatedEvent += OnModelUpdatedEvent;
		}

		public override void PlanTransport()
		{
			throw new NotImplementedException();
		}

		public override void PlanGuard()
		{
			throw new NotImplementedException();
		}

		public List<NavVertex> path;

		/// <summary>
		/// A bit of a hacked-together function at the moment for testing purposes. Takes the worldmodel and creates a path between two distantiated vertices.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void OnModelUpdatedEvent(object sender, EventArgs args)
		{
			var mesh = NavMesh.Generate((from obj in ((WorldModel)sender).Objects select (Obstacle)obj).ToList());


			AvoidSmallPassages(ref mesh); // removes unusable connectivity from the mesh

			foreach (var m in mesh)
				foreach (var e in m.Edges)
					if (e.Polygons.Count == 0)
						continue;

			var edges = ToEdges(mesh);
			var vertices = ToVertices(edges);

			#region setting some start and end vertices for testing
			NavVertex first = null;
			NavVertex last = null;

			foreach (var v in vertices)
			{
				if (v.X < 300 && v.Y < 300)
					first = v;
				if (Math.Abs(v.X - Constants.FrameWidth) < 600 && Math.Abs(v.Y - Constants.FrameHeight) < 600)
					last = v;
			}
			#endregion

			path = WorldProcessing.Planning.Searching.AStarSearch.FindPath(first, last, a => from edge in a.Edges.First().Edges 
																							 select edge.center, Util.Maths.Distance, a => 0).ToList();

			while (RefinePath(ref path)) ; // initial path is between edge centers, this fits it around bends more snugly

			PathPlannedEvent(this, new PathPlannedEventArgs(null,null));
		}

		/// <summary>
		/// Removes connectivity between mesh edges by looking at 3 consecutive edges on a polygon and checking whether the distance of the projection of either point on the first edge onto the last edge is greater than a certain margin, or vice versa, and if so, removes the connectivity of the middle edge to all other edges (that's to say, the middle edge is said to be too small a passage).
		/// This functionality is currently bugged, removing the connectivity of the wrong edges.
		/// </summary>
		/// <param name="mesh"></param>
		private void AvoidSmallPassages(ref List<NavPolygon> mesh)
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

		// It's so ugly I wanna die! Not really but it sure needs refactoring.
		public bool RefinePath(ref List<NavVertex> path)
		{
			bool changed = false;

			while (RefinePathStep(ref path)) changed = true;
			// repeat until no points are changed

			if (!changed)
				return false;

			// width of vehicle ?
			var Margin = 20;

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
				result.Add(edge.center);

			return result;
		}
	}
}
