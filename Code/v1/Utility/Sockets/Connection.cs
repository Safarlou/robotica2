using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Utility.Miscellaneous;
using Utility.Sockets.Messages;

namespace Utility.Sockets
{
	public class ConnectionEventArgs : EventArgs
	{
		public Connection Connection { get; set; }
	}

	public class HandshakeReceivedEventArgs
	{
		public HandshakeMessage HandshakeMessage { get; set; }
	}

	public enum eConnectionState
	{
		Active,
		Closed
	}

	public enum RobotType
	{
		Transport,
		Guard,
		Unknown,
	}

	public class Connection
	{

		private eConnectionState _ConnectionState = eConnectionState.Active;
		public eConnectionState ConnectionState
		{
			get { return _ConnectionState; }
			private set
			{
				if (_ConnectionState == value)
					return;

				_ConnectionState = value;
				RaiseStateChanged();
			}
		}

		private Socket Socket { get; set; }

		#region Events

		public event EventHandler<DebugOutputEventArgs> OnOutput;
		public event EventHandler<MessageEventArgs> MessageReceived;
		public event EventHandler StateChanged;

		#endregion

		public string Description { get { return "Connection"; } }

		internal Connection(Socket socket)
		{
			Socket = socket;

			Read();
		}

		private void Read()
		{
			Socket handler = Socket;

			if (!handler.Connected)
			{
				Close();
				return;
			}

			// Create the state object.
			StateObject state = new StateObject();
			state.workSocket = handler;
			handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
		}

		public void Close()
		{
			if (ConnectionState == eConnectionState.Closed)
				return;

			RaiseOnOutput(new DebugOutput(MessageType.Note, "Connection severed."));
			Socket.Close();
			ConnectionState = eConnectionState.Closed;
		}

		private void ReadCallback(IAsyncResult ar)
		{
			String content = String.Empty;

			// Retrieve the state object and the handler socket
			// from the asynchronous state object.
			StateObject state = (StateObject)ar.AsyncState;
			Socket handler = state.workSocket;

			if (!handler.Connected)
			{
				Close();
				return;
			}

			try
			{
				// Read data from the client socket. 
				int bytesRead = handler.EndReceive(ar);

				if (bytesRead > 0)
				{
					// There  might be more data, so store the data received so far.
					state.sb.Append(Encoding.ASCII.GetString(
						state.buffer, 0, bytesRead));

					// Check for end-of-file tag. If it is not there, read 
					// more data.
					content = state.sb.ToString();
					Debug.Assert(content.IndexOf("<EOF>") > -1);
					content = content.Substring(0, content.Length - "<EOF>".Length);
					
					// All the data has been read from the 
					// client. Display it on the console.
					RaiseOnOutput(new DebugOutput(MessageType.Debug, String.Format("Read {0} bytes from socket.", content.Length)));

					Message msg = MessageHelper.Instance.DeserializeMessage(content);

					RaiseMessageReceived(msg);

					Read();

					/*
					else
					{
						// Not all data received. Get more.
						handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
						new AsyncCallback(ReadCallback), state);
					}*/
				}
			}
			catch (Exception)
			{
				Close();
				return;
			}
		}

		public void Send(Message message)
		{
			String msgText = MessageHelper.Instance.SerializeMessage(message);

			String data = msgText + "<EOF>";
			Send(Socket, data);
		}

		private void Send(Socket handler, String data)
		{
			if (!handler.Connected)
			{
				Close();
				return;
			}

			// Convert the string data to byte data using ASCII encoding.
			byte[] byteData = Encoding.ASCII.GetBytes(data);

			// Begin sending the data to the remote device.
			handler.BeginSend(byteData, 0, byteData.Length, 0,
				new AsyncCallback(SendCallback), handler);
		}

		private void SendCallback(IAsyncResult ar)
		{
			if (!Socket.Connected)
			{
				Close();
				return;
			}

			try
			{
				// Retrieve the socket from the state object.
				Socket handler = (Socket)ar.AsyncState;

				// Complete sending the data to the remote device.
				int bytesSent = handler.EndSend(ar);
				RaiseOnOutput(new DebugOutput(MessageType.Note, String.Format("Sent {0} bytes to client.", bytesSent)));

				//handler.Shutdown(SocketShutdown.Both);
				//handler.Close();

				//handler.BeginReceive(

			}
			catch (Exception e)
			{
				RaiseOnOutput(new DebugOutput(MessageType.Note, String.Format(e.ToString())));
			}
		}

		private void RaiseOnOutput(DebugOutput message)
		{
			if (OnOutput != null)
				OnOutput(this, new DebugOutputEventArgs(message));
		}

		private void RaiseMessageReceived(Message message)
		{
			if (MessageReceived != null)
				MessageReceived(this, new MessageEventArgs() { Message = message} );
		}

		private void RaiseStateChanged()
		{
			if (StateChanged != null)
				StateChanged(this, null);
		}
	}
}
