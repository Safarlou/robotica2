using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Utility.Sockets.Messages
{
	/// <summary>
	/// Robot stops driving forward, turns the desired angle (positive angle is counter-clockwise, negative angle 
	/// is clockwise), stops turning and starts driving forward with the desired speed.
	/// </summary>
	[Obsolete("Probably not going to be used", false)]
	public class MoveAndTurnMessage : Message
	{
		[XmlAttribute]
		public override eMessageType MessageType
		{
			get { return eMessageType.Move; }
		}

		[XmlAttribute]
		public float Angle { get; set; } //radians

		[XmlAttribute]
		public float Speed { get; set; }

		public MoveAndTurnMessage(float angle, float speed)
		{
			Debug.Assert(angle >= -(2.0f * System.Math.PI) && angle <= (2.0f * System.Math.PI), "The Move Message requires an angle value between 0 and 360 degrees.");
			Debug.Assert(speed >= 0.0f && speed <= 1.0f, "The Move Message requires a speed value between 0.0 and 1.0, ie percentages of maximum speed.");

			Angle = angle;
			Speed = speed;
		}
	}
}
