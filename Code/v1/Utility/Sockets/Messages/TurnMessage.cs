using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Utility.Sockets.Messages
{
	public class TurnMessage : Message
	{
		[XmlAttribute]
		public override eMessageType MessageType
		{
			get { return eMessageType.Turn; }
		}

		public enum Direction {Left, Right,}

		[XmlAttribute]
		public Direction TurnDirection { get; set; } //Direction

		public TurnMessage(Direction direction)
		{
			this.TurnDirection = direction;
		}
	}
}
