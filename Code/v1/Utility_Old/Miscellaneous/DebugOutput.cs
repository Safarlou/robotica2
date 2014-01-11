using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Miscellaneous
{
    public enum MessageType
    {
        Note,
        Debug,
        Warning,
        Error
    }

    public class DebugOutput
    {
        public String Message { get; set; }
        public MessageType MessageType { get; private set; }

        public DebugOutput(MessageType messageType, String message = "")
        {
            MessageType = messageType;
            Message = message;
        }

    }

    public class DebugOutputEventArgs : EventArgs
    {
        public DebugOutput Message { get; private set; }

        public DebugOutputEventArgs(DebugOutput message)
        {
            Message = message;
        }

    }

}
