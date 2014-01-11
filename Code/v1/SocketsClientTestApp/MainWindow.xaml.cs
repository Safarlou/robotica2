using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utility.Miscellaneous;
using Utility.Sockets;
using Utility.Sockets.Messages;

namespace SocketsClientTestApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public ClientConnector ClientConnector { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void RaisePropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}


		private ObservableCollection<DebugOutput> _debugMessages = new ObservableCollection<DebugOutput>();
		public ObservableCollection<DebugOutput> DebugMessages { get { return _debugMessages; } }

		private Connection _ActiveConnection = null;
		public Connection ActiveConnection
		{
			get { return _ActiveConnection; }
			set
			{
				if (_ActiveConnection == value)
					return;

				if (_ActiveConnection != null)
				{
					_ActiveConnection.MessageReceived   -= _ActiveConnection_MessageReceived;
					_ActiveConnection.OnOutput          -= _ActiveConnection_OnOutput;
					_ActiveConnection.StateChanged      -= _ActiveConnection_StateChanged;
				}

				_ActiveConnection = value;
				
				if (_ActiveConnection != null)
				{
					_ActiveConnection.MessageReceived   += _ActiveConnection_MessageReceived;
					_ActiveConnection.OnOutput          += _ActiveConnection_OnOutput; 
					_ActiveConnection.StateChanged      += _ActiveConnection_StateChanged;
				}
				
				RaisePropertyChanged("ActiveConnection");
			}
		}

		void _ActiveConnection_StateChanged(object sender, EventArgs e)
		{
			if (ActiveConnection.ConnectionState == eConnectionState.Closed)
				DispatchToForm(() => ActiveConnection = null);
		}

		void _ActiveConnection_MessageReceived(object sender, Utility.Sockets.Messages.MessageEventArgs e)
		{
			TextMessage msg = (TextMessage) e.Message;
			DispatchToForm( () => DebugMessages.Add( new DebugOutput(MessageType.Note, msg.Text)) );
		}

		void _ActiveConnection_OnOutput(object sender, DebugOutputEventArgs e)
		{
			DispatchToForm( () => DebugMessages.Add(e.Message) );
		} 
		
		public MainWindow()
		{
			InitializeComponent();

			DebugMessages.Add(new DebugOutput(MessageType.Note, "Initializing Application"));

			//ClientConnector = new ClientConnector("25.189.184.126");
			ClientConnector = new ClientConnector("127.0.0.1");

			ClientConnector.OnOutput += ClientConnection_OnOutput;
			ClientConnector.ConnectionCreated += ClientConnector_ConnectionCreated;

			ClientConnector.Start();
		}

		void ClientConnector_ConnectionCreated(object sender, ConnectionEventArgs e)
		{
			DispatchToForm(() => ActiveConnection = e.Connection);
		}

		private void DispatchToForm(Action action)
		{
			this.Dispatcher.BeginInvoke(action);
		}

		void ClientConnection_OnOutput(object sender, DebugOutputEventArgs e)
		{
			try
			{
				DispatchToForm(() => DebugMessages.Add(e.Message));
			}
			catch (Exception)
			{
				return;
			}
		}

		private void msgInput_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (ActiveConnection == null)
				{
					MessageBox.Show("Please make sure there is an active connection before sending a message.");
					return;
				}

				String msgtext = msgInput.Text;
				msgInput.Text = "";

				ActiveConnection.Send(new TextMessage() { Text = msgtext });
				//ActiveConnection.Send(new MoveMessage(0.3f, 0.5f));
				return;
			}
		}
	}
}
