using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Utility.Sockets.Messages
{
    public class TextMessage : Message
    {
        [XmlAttribute]
        public override eMessageType MessageType
        {
            get { return eMessageType.Text; }
        }

        [XmlAttribute]
        public string Text { get; set; }

        public TextMessage()
        {

        }
    }
}
