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

		[XmlAttribute]
		public float Angle { get; set; } //radians

		public TurnMessage(float angle)
		{
			Debug.Assert(angle >= -(2 * System.Math.PI) && angle <= (2 * System.Math.PI), "Angle should be between -2pi and 2pi.");

			this.Angle = angle;
		}
	}
}
