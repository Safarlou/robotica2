using System.Collections.Generic;
using WorldProcessing.src.Planning.Actions;

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
			//TODO
			return null;
		}
	}
}
