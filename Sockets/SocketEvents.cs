using System;

namespace Chesham.Sockets
{
    public abstract class SocketClientEvent : EventArgs
    {
        public bool isFaulted => exception != null;

        public Exception exception { get; internal set; }
    }

    public class OnSocketAccepted : SocketClientEvent
    {
        public SocketConnection connection { get; internal set; }

        public bool isAccept { get; set; } = false;
    }

    public class OnSocketReceived : SocketClientEvent
    {
        public SocketConnection connection { get; internal set; }

        public byte[] buffer { get; internal set; }
    }

    public class OnSocketSent : SocketClientEvent
    {
    }

    public class OnSocketDisconnect : SocketClientEvent
    {
    }

}
