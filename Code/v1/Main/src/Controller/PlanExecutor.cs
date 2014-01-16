using System;
using System.Diagnostics;
using WorldProcessing.Representation;

namespace WorldProcessing.Controller
{
	public class PlanExecutor
	{
		public WorldProcessing.Representation.WorldModel WorldModel { get; private set; }

		#region Some helper boundaries and variables

		//private static int fastTurnLimit = 50;

		private NXTController Transport, Guard;

		#endregion

		public PlanExecutor(Planning.Planner planner, WorldProcessing.Representation.WorldModel worldModel, NXTController transport, NXTController guard)
		{
			this.WorldModel = worldModel;
			this.Transport = transport;
			this.Guard = guard;

			planner.PathPlannedEvent += OnPathPlannedEvent;
		}

		private void OnPathPlannedEvent(object sender, Planning.PathPlannedEventArgs args)
		{
			//Precondition: make sure the robot is actually connected
			if (!Transport.Connected || !Guard.Connected) return;

			// Get actions from event arguments
			var transportAction = args.TransportRobotAction;
			var guardAction = args.GuardRobotAction;

			//We've now got 2 actions, one for each robot; let's start differentiating stuff :D
			//First looking at the transportAction
			//HandleTransportAction(transportAction);
			HandleAction(transportAction, Constants.ObjectType.TransportRobot);

			//Now look at the guardAction
			//HandleGuardAction(guardAction);
			//HandleAction(guardAction, Constants.ObjectType.GuardRobot);
		}

		#region Method to handle actions

		private void HandleAction(Planning.Actions.Action action, Constants.ObjectType robot)
		{
			NXTController bot = null;
			Robot modelBot = null;
			if (robot == Constants.ObjectType.TransportRobot) { bot = Transport; modelBot = WorldModel.TransportRobot; }
			else if (robot == Constants.ObjectType.GuardRobot) { bot = Guard; modelBot = WorldModel.GuardRobot; }

			Debug.Assert(bot != null, "Wrong object type");

			if (action.Type == Planning.Actions.ActionType.Move)
			{
				var _action = (Planning.Actions.MovementAction)action;
				var destination = _action.Position;
				double angleOffset = 0;
				if (robot == Constants.ObjectType.TransportRobot)
				{ 
					var distanceVector = new System.Windows.Point(destination.X - WorldModel.TransportRobot.Position.X, 
						destination.Y - WorldModel.TransportRobot.Position.Y);
					var angle = Util.Maths.Angle(new System.Windows.Point(1, 0), distanceVector);
					angleOffset = angle - WorldModel.TransportRobot.Orientation;
				}
				else if (robot == Constants.ObjectType.GuardRobot)
				{
					var distanceVector = new System.Windows.Point(destination.X - WorldModel.GuardRobot.Position.X,
						destination.Y - WorldModel.GuardRobot.Position.Y);
					var angle = Util.Maths.Angle(new System.Windows.Point(1, 0), distanceVector);
					angleOffset = angle - WorldModel.GuardRobot.Orientation;
				}

				//Compare to margin
				if (Math.Abs(angleOffset) > Constants.OrientationMargin)
				{
					if (angleOffset < 0)
					{
						//Make robot turn left pl0x
						//Debug stuff:
						//End debug stuff
						int speed = Constants.TurnSpeed;//Math.Abs(angleOffset) < fastTurnLimit ? slowTurnSpeed : normalTurnSpeed;
						Console.WriteLine(bot.BrickName + " received turnleft action");
						Console.WriteLine("Angle offset: " + angleOffset);
						Console.WriteLine("Brick orientation: " + modelBot.Orientation);
						Console.WriteLine();
						bot.TurnLeft(speed);
					}
					else
					{
						//Make robot turn right pl0x 
						//Debug stuff:
						//End debug stuff
						int speed = Constants.TurnSpeed;//Math.Abs(angleOffset) < fastTurnLimit ? slowTurnSpeed : normalTurnSpeed;
						Console.WriteLine(bot.BrickName + " received turnright action");
						Console.WriteLine("Angle offset: " + angleOffset);
						Console.WriteLine("Brick orientation: " + modelBot.Orientation);
						Console.WriteLine();
						bot.TurnRight(speed);
					}
				}
				else
				{
					//Make robot drive forward pl0x
					//Debug stuff:
					Console.WriteLine(bot.BrickName + " received forward action");
					//End debug stuff
					bot.Forward(Constants.ForwardSpeed);
				}
			}
			else if (action.Type == Planning.Actions.ActionType.Wait)
			{
				//Debug stuff:
				Console.WriteLine(bot.BrickName + " received wait action");
				//End debug stuff
				//bot.Stop();
			}
		}

		#endregion

	}
}
