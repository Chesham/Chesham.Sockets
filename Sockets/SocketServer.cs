using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chesham.Sockets
{
    using SystemSocket = System.Net.Sockets.Socket;

    public class SocketServer : Socket
    {
        public void Listen(EndPoint endPoint, CancellationToken cancellationToken = default)
        {
            var socket = new SystemSocket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endPoint);
            socket.Listen();
            var socketAsyncEvent = new SocketAsyncEventArgs();
            socketAsyncEvent.UserToken = socket;
            socketAsyncEvent.Completed += OnSocketAccepted;
            if (!socket.AcceptAsync(socketAsyncEvent))
            {
                OnSocketAccepted(this, socketAsyncEvent);
            }
        }

        protected bool OnAccept(SystemSocket acceptedSocket, out OnSocketAccepted acceptedEvent)
        {
            acceptedEvent = default;
            var e = new OnSocketAccepted
            {
                connection = new SocketConnection
                {
                    socket = acceptedSocket
                }
            };
            try
            {
                InvokeEvent(this, e);
                if (e.isAccept)
                {
                    acceptedEvent = e;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void OnSocketAccepted(object sender, SocketAsyncEventArgs e)
        {
            Debug.Assert(e.LastOperation == SocketAsyncOperation.Accept);
            if (e.LastOperation != SocketAsyncOperation.Accept)
                return;
            var acceptedSocket = e.AcceptSocket;
            e.AcceptSocket = null;
            if (OnAccept(acceptedSocket, out var acceptedEvent))
            {
                var socketAsyncEvent = new SocketAsyncEventArgs
                {
                    UserToken = acceptedEvent.connection
                };
                socketAsyncEvent.Completed += OnSocketCompleted;
                socketAsyncEvent.SetBuffer(new byte[bufferSize]);
                if (!acceptedSocket.ReceiveAsync(socketAsyncEvent))
                {
                    OnSocketCompleted(sender, socketAsyncEvent);
                }
            }
            else
            {
                acceptedSocket.Dispose();
            }
            var listenSocket = e.UserToken as SystemSocket;
            if (!listenSocket.AcceptAsync(e))
            {
                OnSocketAccepted(sender, e);
            }
        }
    }
}
