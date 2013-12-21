using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.src.Planning
{
    /// <summary>
    /// A plan contains a series of Actions and some useful functionality.
    /// </summary>
    public class Plan
    {
        public List<Action> Actions { get; private set; }

        private int _counter;

        public Plan()
        {
            _counter = 0;
        }

        public Action NextAction()
        {
            if (_counter >= Actions.Count)
                return new NoAction();
            else if (Actions[_counter].State == ActionState.Complete)
                return Actions[_counter++];
            else
                return Actions[_counter];
        }
    }
}
