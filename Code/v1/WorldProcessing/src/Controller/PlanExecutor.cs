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
		private Planning.Plan transportPlan;
		private Planning.Plan guardPlan;

		public Object _lock = new Object();
		private bool keepRunning = true;
		public bool KeepRunning 
		{ 
			get { lock (_lock) return keepRunning; }
			set { lock (_lock) keepRunning = value; } 
		}

		public PlanExecutor(Representation.WorldModel worldModel, Planning.Plan transportPlan, Planning.Plan guardPlan)
		{
			this.worldModel = worldModel;
			this.transportPlan = transportPlan;
			this.guardPlan = guardPlan;
		}

		public bool Execute()
		{
			if (worldModel == null || transportPlan == null || guardPlan == null) return false;
			new Thread(new ThreadStart(ExecutePlan)).Start();
			return true;
		}

		private void ExecutePlan()
		{
			while (KeepRunning)
			{
				Thread transportThread = null;
				Thread guardThread = null;
				Planning.Action currentTransportAction = transportPlan.NextAction();
				Planning.Action currentGuardAction = guardPlan.NextAction();
				//Initiate transport action
				if (currentTransportAction.State == Planning.ActionState.Due)
				{
					currentTransportAction.Start();
					//Start thread to run loop for movement
					//if action is movement action, 
						//start thread with evaluation of movement action for transport bot
					if (currentTransportAction.GetType() == typeof(Planning.MovementAction))
					{
						transportThread = new Thread(() => ExecuteMovementAction(worldModel.Robots[0]));
						transportThread.Start();
					}
					// else do other cases
				}

				//Initiate guard action
				if (currentGuardAction.State == Planning.ActionState.Due)
				{
					currentGuardAction.Start();
					//Start thread to run loop for whatever action
					//if action is movement action, 
						//start thread with evaluation of movement action for transport bot
					//else if action is clear action
						//clear dat bitch
					//etc
					if (currentGuardAction.GetType() == typeof(Planning.MovementAction))
					{
						guardThread = new Thread(() => ExecuteMovementAction(worldModel.Robots[1]));
						guardThread.Start();
					}
					// else do other cases
				}

				//Join both threads (wait until both actions done???)
				//Maybe not such a good idea but oh well
				if (transportThread != null) transportThread.Join();
				if (guardThread != null) guardThread.Join();

				//Action is completed, terminate
				currentTransportAction.End();
				currentGuardAction.End();
			}
		}

		private void ExecuteNoAction(Representation.Robot robot)
		{
			//TODO
		}

		private void ExecuteWaitAction()
		{
			Representation.TransportRobot bot = (Representation.TransportRobot)worldModel.Robots[0];
			//TODO: maek the transbot wait for if necesaryr maybe even other time no?
		}

		private void ExecuteMovementAction(Representation.Robot robot)
		{
			//Determine robot type
			if (robot.ObjectType == Constants.ObjectType.TransportRobot)
			{
				//ASSUMING [0] IS THE TRANSPORT ROBOT
				Representation.TransportRobot bot = (Representation.TransportRobot)worldModel.Robots[0];
				//TODO: ACCUALY MAEK BOT MOEV
			}
			else if (robot.ObjectType == Constants.ObjectType.GuardRobot)
			{
				//ASSUMING [1] IS THE GUARD ROBOT
				Representation.GuardRobot bot = (Representation.GuardRobot)worldModel.Robots[1];
				//TODO: DRIVING UP IN THIS BITCH
			}
		}

		/// <summary>
		/// Only executable by guard bot so no parameter here
		/// </summary>
		private void ExecuteClearAction()
		{
			Representation.GuardRobot bot = (Representation.GuardRobot)worldModel.Robots[1];
			//TODO maek clear actino the rbot do that
		}
	}
}
