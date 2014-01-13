using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Utility.Sockets.Messages
{
	/// <summary>
	/// Robot stops moving.
	/// </summary>
	public class StopMessage : Message
	{
		[XmlAttribute]
		public override eMessageType MessageType
		{
			get { return eMessageType.Stop; }
		}
		
		public StopMessage()
		{

		}
	}
}
