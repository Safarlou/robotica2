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
		public Representation.WorldModel WorldModel { get; private set; }
		

		#region Some helper boundaries and variables
		
		// Amount of degrees off that's still ok
		private double orientationMarginDegrees;
		private double orientationMargin;
		
		//TODO: Create dictionary of actions and their types in order to convert easily

		#endregion

		public PlanExecutor(Planning.Planner planner, Representation.WorldModel worldModel)
		{
			this.WorldModel = worldModel;
			this.orientationMarginDegrees = 5;
			this.orientationMargin = System.Math.PI * orientationMarginDegrees / 180.0;

			//Create connections collection
			_ClientConnections = new System.Collections.Generic.Dictionary<Utility.Sockets.ConnectionType, ConnectionData>();
			
			//Create socket
			this.RobotSocket = new Utility.Sockets.ServerConnector();
			this.RobotSocket.ConnectionCreated += RobotSocket_ConnectionCreated;
			//Start socket
			this.RobotSocket.Start();

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
			double currentTransportRobotOrientation = WorldModel.Robots[0].Orientation;
			double currentGuardRobotOrientation = WorldModel.Robots[1].Orientation;

			// Get current position on both bots
			// [0] = transport bot, [1] = guard bot
			System.Windows.Point currentTransportRobotPosition = WorldModel.Robots[0].Position;
			System.Windows.Point currentGuardRobotPosition = WorldModel.Robots[1].Position;

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

		#region Socket stuff

		private void SendMessage(Utility.Sockets.Messages.Message message, ConnectionData robotConnection)
		{
			if (robotConnection == null)
			{
				System.Console.WriteLine("Connection is inactive!");
			}
			robotConnection.Connection.Send(message);
		}

		public Utility.Sockets.ServerConnector RobotSocket { get; private set; }

		private System.Collections.Generic.Dictionary<Utility.Sockets.ConnectionType, ConnectionData> _ClientConnections;
		public System.Collections.Generic.Dictionary<Utility.Sockets.ConnectionType, ConnectionData> ClientConnections { get { return _ClientConnections; } }

		private void RobotSocket_ConnectionCreated(object sender, Utility.Sockets.ConnectionEventArgs e)
		{
			ClientConnections.Add(e.ConnectionType, new ConnectionData(e.Connection));
		}

		#endregion
	}

	#region Moar socket stuff

	public class ConnectionData
	{
		public Utility.Sockets.Connection Connection { get; private set; }
		public System.Collections.ObjectModel.ObservableCollection<Utility.Sockets.Messages.Message> Messages { get; private set; }

		public ConnectionData(Utility.Sockets.Connection conn)
		{
			this.Connection = conn;

			conn.MessageReceived += conn_MessageReceived;
			conn.StateChanged += conn_StateChanged;
			conn.OnOutput += conn_OnOutput;
		}

		private void conn_OnOutput(object sender, Utility.Miscellaneous.DebugOutputEventArgs e)
		{
			System.Console.WriteLine(e.Message);
		}

		private void conn_StateChanged(object sender, EventArgs e)
		{
			if (Connection.ConnectionState == Utility.Sockets.eConnectionState.Closed)
			{
				System.Console.WriteLine("Connection closed!");
			}
		}

		private void conn_MessageReceived(object sender, Utility.Sockets.Messages.MessageEventArgs e)
		{
			this.Messages.Add(e.Message);
		}
	}
	
	#endregion
}
