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

			Console.WriteLine(mesh.Count);

			var points = new List<NavVertex>();// Util.Geo.PolygonsToEdgePoints(mesh);

			NavVertex first = new NavVertex(new System.Windows.Point());
			NavVertex last = new NavVertex(new System.Windows.Point());

			foreach (var p in points)
			{
				if (p.X < 300 && p.Y < 300)
					first = p;
				if (Math.Abs(p.X - Constants.FrameWidth) < 600 && Math.Abs(p.Y - Constants.FrameHeight) < 600)
					last = p;
			}

			//first.X += 50;
			//first.Y += 50;
			//last.X -= 50;
			//last.Y -= 50;

			//path = WorldProcessing.Planning.Searching.AStarSearch.FindPath(first, last, Util.Maths.Distance, a => 0).ToList();

			PathPlannedEvent(this, new EventArgs());
		}
	}
}
