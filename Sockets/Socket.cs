using System;

namespace Chesham.Sockets
{
    public abstract class Socket
    {
        public event EventHandler<SocketClientEvent> OnEvent;

        protected bool OnAccept(System.Net.Sockets.Socket acceptedSocket)
        {
            var e = new OnSocketAccepted
            {
                socket = acceptedSocket
            };
            try
            {
                OnEvent?.Invoke(this, e);
                return e.isAccept;
            }
            catch
            {
                return false;
            }
        }

        protected void OnReceive(byte[] buffer)
        {
            var e = new OnSocketReceived
            {
                buffer = buffer
            };
            try
            {
                OnEvent?.Invoke(this, e);
            }
            catch
            {
            }
        }
    }
}
