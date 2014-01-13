using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Utility.Sockets.Messages
{
    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }

    public enum eMessageType
    {
        Text,
    }

    [XmlRoot]
    public abstract class Message
    {
        [XmlAttribute]
        public abstract eMessageType MessageType { get; }

        public Message()
        {

        }
    }
}
