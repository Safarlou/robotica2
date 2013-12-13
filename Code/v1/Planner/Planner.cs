using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldModel;

namespace Planning
{
    class Planner : AbstractPlanner
    {
        public Planner(Model model) : base(model)
        {
            //LOL
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
