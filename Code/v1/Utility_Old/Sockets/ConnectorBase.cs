using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Miscellaneous;

namespace Utility.Sockets
{
    public abstract class ConnectorBase
    {
        

        internal ConnectorBase()
        {

        }

        public abstract void Start();
        public abstract void Shutdown();

        public event EventHandler<DebugOutputEventArgs> OnOutput;
        public event EventHandler<ConnectionEventArgs> ConnectionCreated;

        protected void RaiseOnOutput(DebugOutput message)
        {
            if (OnOutput != null)
                OnOutput(this, new DebugOutputEventArgs(message));
        }

        protected void RaiseConnectionCreated(Connection connection)
        {
            RaiseOnOutput(new DebugOutput(MessageType.Note, "Connection created."));

            if (ConnectionCreated != null)
                ConnectionCreated(this, new ConnectionEventArgs() { Connection = connection });
        }
    }
}
