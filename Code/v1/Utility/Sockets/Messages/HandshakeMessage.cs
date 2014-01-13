using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Utility.Sockets.Messages
{
	public class HandshakeMessage : Message
	{
		[XmlAttribute]
		public override eMessageType MessageType
		{
			get { return eMessageType.Handshake; }
		}

		[XmlAttribute]
		public RobotType Robot { get; set; }

		public HandshakeMessage() { }
	}
}
