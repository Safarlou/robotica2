using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Utility.Sockets.Messages
{
    public class MessageHelper
    {
        private static MessageHelper _instance = null;
        public static MessageHelper Instance { get { if (_instance == null) _instance = new MessageHelper(); return _instance; } }

        private Dictionary<eMessageType, Type> messageTypesToTypes = new Dictionary<eMessageType, Type>();

        private MessageHelper()
        {
            Register(eMessageType.Text, typeof(TextMessage));
        }

        public void Register(eMessageType messageType, Type type)
        {
            messageTypesToTypes.Add(messageType, type);
        }

        public String SerializeMessage(Message message)
        {
            XmlSerializer srl = new XmlSerializer(typeof(Message), messageTypesToTypes.Values.ToArray());
            
            TextWriter contentWriter = new StringWriter();

            srl.Serialize(contentWriter, message);

            String result = contentWriter.ToString();

            return result;
        }

        public Message DeserializeMessage(String xml)
        {
            XmlSerializer srl = new XmlSerializer(typeof(Message), messageTypesToTypes.Values.ToArray());

            TextReader contentReader = new StringReader(xml);

            Message msg = (Message) srl.Deserialize(contentReader);

            return msg;
        }
    }
}
