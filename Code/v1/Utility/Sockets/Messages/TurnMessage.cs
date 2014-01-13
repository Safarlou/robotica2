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

		[XmlAttribute]
		public float Angle { get; set; }

		public TurnMessage(Direction direction, float angle)
		{
			Debug.Assert(angle >= -(2 * Math.PI) && angle <= (2 * Math.PI), 
				"Angle should not exceed 360 degrees (or 2pi radians in this case)");
			this.TurnDirection = direction;
		}
	}
}
