using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorldProcessing.src.Controller
{
	public class PlanExecutor
	{
		private Representation.WorldModel worldModel;

		#region Some helper boundaries and variables
		// Amount of degrees off that's still ok
		private double orientationMarginDegrees;
		private double orientationMargin;
		#endregion

		public PlanExecutor(Planning.Planner planner, Representation.WorldModel worldModel)
		{
			this.worldModel = worldModel;
			this.orientationMarginDegrees = 5;
			this.orientationMargin = System.Math.PI * orientationMarginDegrees / 180.0;

			planner.PathPlannedEvent += OnPathPlannedEvent;
		}

		private void OnPathPlannedEvent(object sender, Planning.PathPlannedEventArgs args)
		{
			// Get actions from event arguments
			Planning.MovementAction transportAction = (Planning.MovementAction)args.TransportRobotAction;
			Planning.MovementAction guardAction = (Planning.MovementAction)args.GuardRobotAction;

			/*
			 * Account for different action types of guard robot...
			 * Controller should act differently according to these different action types.
			 * 
			 * OR
			 * 
			 * Generalise all actions to movement actions, that way we only make robots drive around.
			 */

			// Get current orientation on both bots
			// [0] = transport bot, [1] = guard bot
			double currentTransportRobotOrientation = worldModel.Robots[0].Orientation;
			double currentGuardRobotOrientation = worldModel.Robots[1].Orientation;

			// Get current position on both bots
			// [0] = transport bot, [1] = guard bot
			System.Windows.Point currentTransportRobotPosition = worldModel.Robots[0].Position;
			System.Windows.Point currentGuardRobotPosition = worldModel.Robots[1].Position;

			// Determine if change of course is necessary for transport robot
			double angleOfTransportRobotWithDestination = Util.Maths.Angle(currentTransportRobotPosition, transportAction.Position);
			if (System.Math.Abs(angleOfTransportRobotWithDestination) > orientationMargin)
			{
				//TODO: fix orientation
			}

			// Determine if change of course is necessary for guard robot
			double angleOfGuardRobotWithDestination = Util.Maths.Angle(currentGuardRobotPosition, guardAction.Position);
			if (System.Math.Abs(angleOfGuardRobotWithDestination) > orientationMargin)
			{
				//TODO: fix orientation
			}

			/*
			 * Furthermore: actions should have more information on what to do in order to implement this part efficiently.
			 * Do I wait, do I drive, do I turn, et cetera... These are all things that the planner determines and it 
			 * thus should transmit this data in some way, I think. Let me know what you think yo
			 * - Koen
			 */
		}
	}
}
