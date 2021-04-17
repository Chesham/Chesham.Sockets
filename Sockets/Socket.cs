using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace Chesham.Sockets
{
    using SystemSocket = System.Net.Sockets.Socket;

    public abstract class Socket
    {
        public event EventHandler<SocketClientEvent> OnEvent;

        protected void InvokeEvent(object sender, SocketClientEvent e)
        {
            OnEvent?.Invoke(sender, e);
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

        protected void OnSocketCompleted(object sender, SocketAsyncEventArgs e)
        {
            var socket = e.UserToken as SystemSocket;
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                    {
                        OnReceive(e.MemoryBuffer.Slice(e.Offset, e.BytesTransferred).ToArray());
                        if (!socket.ReceiveAsync(e))
                        {
                            OnSocketCompleted(sender, e);
                        }
                    }
                    else
                    {
                        socket.Dispose();
                    }
                    break;
                case SocketAsyncOperation.Send:
                    break;
                case SocketAsyncOperation.Disconnect:
                    break;
                default:
                    Debug.Assert(false, "Unexpect operation occured");
                    break;
            }
        }
    }
}
