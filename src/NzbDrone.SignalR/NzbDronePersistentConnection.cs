using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.SignalR
{
    public interface IBroadcastSignalRMessage
    {
        void BroadcastMessage(SignalRMessage message);
    }

    public sealed class NzbDronePersistentConnection : PersistentConnection, IBroadcastSignalRMessage
    {
        private IPersistentConnectionContext Context
        {
            get
            {
                return ((ConnectionManager)GlobalHost.ConnectionManager).GetConnection(GetType());
            }
        }

        public void BroadcastMessage(SignalRMessage message)
        {
            Context.Connection.Broadcast(message);
        }
    }
}