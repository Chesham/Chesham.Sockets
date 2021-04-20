using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Chesham.Sockets
{
    public class SocketConnection : Socket
    {
        public override event EventHandler<SocketClientEvent> OnEvent
        {
            add
            {
                base.OnEvent += value;
                if (socket != null)
                {
                    ReceiveAsync();
                }
            }
            remove
            {
                base.OnEvent -= value;
            }
        }

        public System.Net.Sockets.Socket socket { get; internal set; }

        public async ValueTask<int> SendAsync(ReadOnlyMemory<byte> payload, SocketFlags socketFlags = SocketFlags.None, CancellationToken cancelToken = default)
        {
            return await socket.SendAsync(payload, socketFlags, cancelToken);
        }

        public void Close()
        {
            socket?.Close();
        }

        public async Task ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken = default)
        {
            if (this.socket != null)
                throw new InvalidOperationException("Socket already established");
            var socket = new System.Net.Sockets.Socket(SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(endPoint, cancellationToken);
            this.socket = socket;
            if (hasEvents)
            {
                ReceiveAsync();
            }
        }

        private void ReceiveAsync()
        {
            var socketEventArgs = new SocketAsyncEventArgs
            {
                UserToken = this,
            };
            socketEventArgs.Completed += OnSocketCompleted;
            socketEventArgs.SetBuffer(new byte[bufferSize]);
            if (!socket.ReceiveAsync(socketEventArgs))
            {
                OnSocketCompleted(this, socketEventArgs);
            }
        }

        ~SocketConnection()
        {
            Close();
        }
    }
}
