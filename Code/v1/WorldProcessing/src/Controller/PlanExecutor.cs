using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorldProcessing;

namespace WorldProcessing.src.Controller
{
	public class PlanExecutor
	{
		public Representation.WorldModel WorldModel { get; private set; }
		

		#region Some helper boundaries and variables
		
		// Amount of degrees off that's still ok
		private double orientationMarginDegrees;
		private double orientationMargin;
		#endregion

		public PlanExecutor(Planning.Planner planner, Representation.WorldModel worldModel)
		{
			this.WorldModel = worldModel;
			this.orientationMarginDegrees = 5;
			this.orientationMargin = System.Math.PI * orientationMarginDegrees / 180.0;

			//Create connections collection
			_ClientConnections = new System.Collections.Generic.Dictionary<Utility.Sockets.RobotType, ConnectionData>();
			
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
			var transportAction = args.TransportRobotAction;
			var guardAction = args.GuardRobotAction;

			//We've now got 2 actions, one for each robot; let's start differentiating stuff :D
			//First looking at the transportAction
			HandleTransportAction(transportAction);

			//Now look at the guardAction
			HandleGuardAction(guardAction);
		}

		private void HandleGuardAction(Planning.Actions.Action guardAction)
		{
			if (guardAction.Type == Planning.Actions.ActionType.Move)
			{
				Planning.Actions.MovementAction action = (Planning.Actions.MovementAction)guardAction;
				//CREATE MOVE MESSAGE
			}
			else if (guardAction.Type == Planning.Actions.ActionType.Turn)
			{
				Planning.Actions.TurnAction action = (Planning.Actions.TurnAction)guardAction;
				//CREATE TURN MESSAGE
			}
			else if (guardAction.Type == Planning.Actions.ActionType.Wait)
			{
				Planning.Actions.WaitAction action = (Planning.Actions.WaitAction)guardAction;
				//CREATE STOP MESSAGE
			}
		}

		private void HandleTransportAction(Planning.Actions.Action transportAction)
		{
			if (transportAction.Type == Planning.Actions.ActionType.Move)
			{
				Planning.Actions.MovementAction action = (Planning.Actions.MovementAction)transportAction;
				//CREATE MOVE MESSAGE
			}
			else if (transportAction.Type == Planning.Actions.ActionType.Turn)
			{
				Planning.Actions.TurnAction action = (Planning.Actions.TurnAction)transportAction;
				//CREATE TURN MESSAGE
			}
			else if (transportAction.Type == Planning.Actions.ActionType.Wait)
			{
				Planning.Actions.WaitAction action = (Planning.Actions.WaitAction)transportAction;
				//CREATE STOP MESSAGE
			}
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

		private System.Collections.Generic.Dictionary<Utility.Sockets.RobotType, ConnectionData> _ClientConnections;
		public System.Collections.Generic.Dictionary<Utility.Sockets.RobotType, ConnectionData> ClientConnections 
		{ 
			get 
			{ 
				return _ClientConnections; 
			} 
		}

		private void RobotSocket_ConnectionCreated(object sender, Utility.Sockets.ConnectionEventArgs e)
		{
			//Create new ConnectionData object from event args
			ConnectionData toAdd = new ConnectionData(e.Connection);

			//Subscribe event handler to HandshakeReceived event.
			//The event handler will add the connection with determined robot type to the list of clients
			toAdd.HandshakeReceived += DetermineConnectionType;
			
			//Send handshake message
			toAdd.Connection.Send(new Utility.Sockets.Messages.HandshakeMessage());
		}

		private void DetermineConnectionType(object sender, Utility.Sockets.HandshakeReceivedEventArgs e)
		{
			//Add new connectiondata object to the clientconnections dictionary;
			//	Key = robot type in handshakemessage (guard or transport or unknown)
			//	Value = connectiondata object that received the handshake
			ClientConnections.Add(e.HandshakeMessage.Robot, (ConnectionData)sender);
		}

		#endregion
	}

	#region Moar socket stuff

	public class ConnectionData
	{
		public Utility.Sockets.Connection Connection { get; private set; }
		public System.Collections.ObjectModel.ObservableCollection<Utility.Sockets.Messages.Message> Messages { get; private set; }

		public event EventHandler<Utility.Sockets.HandshakeReceivedEventArgs> HandshakeReceived;

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
			if (e.Message.MessageType == Utility.Sockets.Messages.eMessageType.Handshake)
			{
				HandshakeReceived(this, new Utility.Sockets.HandshakeReceivedEventArgs() 
				{ HandshakeMessage = (Utility.Sockets.Messages.HandshakeMessage)e.Message });
			}
		}
	}
	
	#endregion
}
