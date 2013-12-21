using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.src.Planning.Searching
{
    /// <summary>
    /// Implements a path of nodes.
    /// </summary>
    /// <typeparam name="Node"></typeparam>
    public class Path<Node> : IEnumerable<Node>
    {
        public Node LastStep { get; private set; }
        public Path<Node> PreviousSteps { get; private set; }
        public double TotalCost { get; private set; }

        private Path(Node parent, Path<Node> pathTotal, double totalCost)
        {
            this.LastStep = parent;
            this.PreviousSteps = pathTotal;
            this.TotalCost = totalCost;
        }

        public Path(Node start) : this(start, null, 0) { }

        public Path<Node> AddStep(Node step, double stepCost)
        {
            return new Path<Node>(step, this, TotalCost + stepCost);
        }

        public IEnumerator<Node> GetEnumerator()
        {
            for (Path<Node> p = this; p != null; p = p.PreviousSteps)
                yield return p.LastStep;
        }

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
