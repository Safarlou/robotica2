using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.src.Planning.Actions
{
	public class TurnAction : Action
	{
		public TurnAction()
		{
			this.Type = ActionType.Turn;
		}
	}
}
