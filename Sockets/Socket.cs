using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace Chesham.Sockets
{
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
            var connection = e.UserToken as SocketConnection;
            Debug.Assert(connection != null);
            if (connection == null)
                return;
            var socket = connection.socket;
            Debug.Assert(socket != null);
            if (socket == null)
                return;
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                    {
                        connection.OnEvent?.Invoke(connection, new OnSocketReceived
                        {
                            buffer = e.MemoryBuffer.Slice(e.Offset, e.BytesTransferred).ToArray()
                        });
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

        protected int bufferSize = 8192;
    }
}
