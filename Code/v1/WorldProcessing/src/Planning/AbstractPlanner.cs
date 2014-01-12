using WorldProcessing.Representation;

namespace WorldProcessing.src.Planning
{
    /// <summary>
    /// An abstract class providing method signatures for classes that are
    /// designed to do planning for our robots.
    /// </summary>
    public abstract class AbstractPlanner
    {
        private WorldModel model;

        public AbstractPlanner(WorldModel model)
        {
            this.model = model;
        }

        /// <summary>
        /// Creates a Plan for the transport robot.
        /// </summary>
        public abstract void PlanTransport();

        /// <summary>
        /// Creates a Plan for the guard robot.
        /// </summary>
        public abstract void PlanGuard();
    }
}
