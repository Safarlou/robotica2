using System;
using WorldProcessing.Representation;

namespace WorldProcessing.Planning
{
    /// <summary>
    /// The Planner creates a plan for the transport/guard robots,
    /// according to calculations on the WorldModel.
    /// </summary>
    public class Planner : AbstractPlanner
    {
        public Planner(WorldModel model) : base(model)
        {
            //bla
        }

        public override void PlanTransport()
        {
            throw new NotImplementedException();
        }

        public override void PlanGuard()
        {
            throw new NotImplementedException();
        }
    }
}
