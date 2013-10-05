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
using System.Windows.Threading;
using Utility.Miscellaneous;
using Utility.Sockets;
using Utility.Sockets.Messages;

namespace SocketsTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public class ConnectionData
        {
            public MainWindow Window { get; private set; }
            public Connection Connection { get; private set; }
            public ObservableCollection<Message> Messages { get; private set; }

            public ConnectionData(MainWindow window, Connection conn)
            {
                Window = window;
                Connection = conn;
                Messages = new ObservableCollection<Message>();

                conn.MessageReceived += conn_MessageReceived;
                conn.StateChanged += conn_StateChanged;
                conn.OnOutput += conn_OnOutput;
            }

            void conn_OnOutput(object sender, DebugOutputEventArgs e)
            {
                Window.DispatchToForm(() => Window.DebugMessages.Add(e.Message));
            }

            void conn_StateChanged(object sender, EventArgs e)
            {
                if (Connection.ConnectionState == eConnectionState.Closed)
                    Window.DispatchToForm(() => Window.ClientConnections.Remove(this));
            }

            void conn_ConnectionClosed(object sender, EventArgs e)
            {
            }

            void conn_MessageReceived(object sender, MessageEventArgs e)
            {
                Messages.Add(e.Message);
            }
        }
        
    
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<DebugOutput> _debugMessages = new ObservableCollection<DebugOutput>();
        public ObservableCollection<DebugOutput> DebugMessages { get { return _debugMessages; } }

        public ServerConnector ServerConnector { get; private set; }

        private ObservableCollection<ConnectionData> _clientConnections = new ObservableCollection<ConnectionData>();
        public ObservableCollection<ConnectionData> ClientConnections { get { return _clientConnections; } }

        private ConnectionData _ActiveConnection = null;
        public ConnectionData ActiveConnection
        {
            get { return _ActiveConnection; }
            set
            {
                if (_ActiveConnection == value)
                    return;
                _ActiveConnection = value;
                RaisePropertyChanged("ActiveConnection");
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DebugMessages.Add(new DebugOutput(MessageType.Note, "Initializing Application"));

            ServerConnector = new ServerConnector();

            ServerConnector.OnOutput += ServerConnection_OnOutput;
            ServerConnector.ConnectionCreated += ServerConnection_ConnectionCreated;

            ServerConnector.Start();
        }

        private void RaisePropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void DispatchToForm(Action action)
        {
            this.Dispatcher.BeginInvoke(action);
        }

        void ServerConnection_ConnectionCreated(object sender, ConnectionEventArgs e)
        {
            DispatchToForm(() => ClientConnections.Add(new ConnectionData(this, e.Connection)));
        }

        private void ServerConnection_OnOutput(object sender, DebugOutputEventArgs e)
        {
            DispatchToForm( () => DebugMessages.Add(e.Message) );
        }

        private void messageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ActiveConnection == null)
                {    
                    MessageBox.Show("Select an active connection to send a message through it.");
                    return;
                }

                String input = messageInput.Text;
                messageInput.Text = "";

                ActiveConnection.Connection.Send(new TextMessage() { Text = input });

                return;
            }
        }
    }
}
