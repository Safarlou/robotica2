using System;
using System.Collections.Generic;
using System.Linq;
using WorldProcessing.Representation;

namespace WorldProcessing.Planning
{
	public delegate void PathPlannedEventHandler(object sender, EventArgs e);

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

		public List<NavVertex> path;

		private void OnModelUpdatedEvent(object sender, EventArgs args)
		{
			var mesh = NavMesh.Generate((from obj in ((WorldModel)sender).Objects select (Obstacle)obj).ToList());

			AvoidSmallPolygons(ref mesh);

			var edges = ToEdges(mesh);
			var vertices = ToVertices(edges);

			NavVertex first = null;
			NavVertex last = null;

			foreach (var v in vertices)
			{
				if (v.X < 300 && v.Y < 300)
					first = v;
				if (Math.Abs(v.X - Constants.FrameWidth) < 600 && Math.Abs(v.Y - Constants.FrameHeight) < 600)
					last = v;
			}

			path = WorldProcessing.Planning.Searching.AStarSearch.FindPath(first, last, a => from edge in a.Edges.First().Edges select edge.center, Util.Maths.Distance, a => 0).ToList();

			RefinePath(ref path);

			PathPlannedEvent(this, new EventArgs());
		}

		private void AvoidSmallPolygons(ref List<NavPolygon> mesh)
		{
			foreach (var polygon in mesh)
			{
				int c = polygon.Edges.Count;
				for (int i = 0; i < c; i++)
				{
					var e1 = polygon.Edges[i];

					var e0 = polygon.Edges.Find(new Predicate<NavEdge>(a => a.V0 == e1.V0 || a.V1 == e1.V0));
					var e2 = polygon.Edges.Find(new Predicate<NavEdge>(a => a.V0 == e1.V1 || a.V1 == e1.V1));
					
					var M = 20;

					// TODO: Something about this is not yet working correctly. Firstly, the > 0.001 is necessary because sometimes
					// the distance is super tiny (like 10^-13) but the edge involved should actually not be removed.
					// Additionaly, sometimes edges are removed that shouldn't be. All in all, although sometimes promising,
					// this can lead to pretty strange pathing results.
					if (c > 3)
					{
						if ((Util.Maths.Distance(Util.Nav.Project(e0.V0, e2), e0.V1) < M
							&& Util.Maths.Distance(Util.Nav.Project(e0.V0, e2), e0.V1) > 0.001)
							|| (Util.Maths.Distance(Util.Nav.Project(e0.V1, e2), e0.V1) < M
							&& Util.Maths.Distance(Util.Nav.Project(e0.V1, e2), e0.V1) > 0.001)
							|| (Util.Maths.Distance(Util.Nav.Project(e2.V0, e0), e2.V0) < M
							&& Util.Maths.Distance(Util.Nav.Project(e2.V0, e0), e2.V0) > 0.001)
							|| (Util.Maths.Distance(Util.Nav.Project(e2.V1, e0), e2.V1) < M
							&& Util.Maths.Distance(Util.Nav.Project(e2.V1, e0), e2.V1) > 0.001))
						{
							foreach (var poly in e1.Polygons)
								poly.Edges.Remove(e1);
						}
					}

					// this checks for edges that are simply too short
					if (Util.Maths.Distance(e1.V0, e1.V1) < M)
						foreach (var poly in e1.Polygons)
							poly.Edges.Remove(e1);

					c = polygon.Edges.Count;
				}
			}
		}

		public void RefinePath(ref List<NavVertex> path)
		{
			while (RefinePathStep(ref path)) ;
			// repeat until no points are changed
		}
		public bool RefinePathStep(ref List<NavVertex> path)
		{
			var changed = false;
			// foreach point0,1,2 on path
			int c = path.Count;
			for (int i = 1; i < c - 1; i++)
			{
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
				var M = 25;
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
				var threshold = 1;
				if (Math.Abs(closest.X - v1.X) > threshold || Math.Abs(closest.Y - v1.Y) > threshold)
				{
					v1.X = closest.X;
					v1.Y = closest.Y;
					changed = true; ;
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
