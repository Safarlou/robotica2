using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planning
{
    abstract class AbstractPlanner
    {
        private Input model;

        public AbstractPlanner(Input model)
        {
            this.model = model;
        }

        public abstract void PlanTransport();
        public abstract void PlanGuard();
    }
}
