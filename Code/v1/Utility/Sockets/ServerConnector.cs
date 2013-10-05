using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Utility.Miscellaneous;

namespace Utility.Sockets
{
    

    public class ServerConnector : ConnectorBase
    {
        #region Fields

        private ManualResetEvent allDone = new ManualResetEvent(false);

        private Socket listener = null;

        #endregion

        #region Constructors

        public ServerConnector()
        {
            
        }

        #endregion

        #region Methods

        public override void Start()
        {
            RaiseOnOutput(new DebugOutput(MessageType.Note, "Initializing listening socket"));

            

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //String hostname = Dns.GetHostName();
                //IPHostEntry ipHostInfo = Dns.GetHostEntry("127.0.0.1");
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());

                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

                listener.Bind(localEndPoint);
                listener.Listen(100);

                Listen();
            }
            catch (Exception e)
            {
                RaiseOnOutput(new DebugOutput(MessageType.Error, e.ToString()));
            }
        }

        public override void Shutdown()
        {
            
        }

        private void Listen()
        {
            RaiseOnOutput(new DebugOutput(MessageType.Note, "Listening for connections."));
            
            listener.BeginAccept( new AsyncCallback(AcceptCallback), listener);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            //allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            Connection newConn = new Connection(handler);

            RaiseConnectionCreated(newConn);

            Listen();
        }

        #endregion
    }
}
