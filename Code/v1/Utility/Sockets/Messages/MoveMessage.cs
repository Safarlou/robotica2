using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Utility.Sockets.Messages
{
	public class MoveMessage : Message
	{
		[XmlAttribute]
		public override eMessageType MessageType
		{
			get { return eMessageType.Move; }
		}

		[XmlAttribute]
		public float Speed { get; set; }

		public MoveMessage(float speed)
		{
			Debug.Assert(speed >= 0.0f && speed <= 1.0f, "Speed should be between 0f and 1f.");

			this.Speed = speed;
		}
	}
}
