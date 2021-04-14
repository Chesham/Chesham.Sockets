using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chesham.Sockets
{
    public class SocketServer : Socket
    {
        private HashSet<SocketAsyncEventArgs> socketEvents = new HashSet<SocketAsyncEventArgs>();

        private int bufferSize = 8192;

        public void Listen(EndPoint endPoint, CancellationToken cancelToken)
        {
            var socket = new System.Net.Sockets.Socket(SocketType.Stream, ProtocolType.Tcp);
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

        private void OnSocketAccepted(object sender, SocketAsyncEventArgs e)
        {
            Debug.Assert(e.LastOperation == SocketAsyncOperation.Accept);
            if (e.LastOperation != SocketAsyncOperation.Accept)
                return;
            var acceptedSocket = e.AcceptSocket;
            e.AcceptSocket = null;
            if (OnAccept(acceptedSocket))
            {
                var socketAsyncEvent = new SocketAsyncEventArgs
                {
                    UserToken = acceptedSocket
                };
                socketAsyncEvent.Completed += OnSocketCompleted;
                socketAsyncEvent.SetBuffer(new byte[bufferSize]);
                if (!acceptedSocket.ReceiveAsync(socketAsyncEvent))
                {
                    OnSocketCompleted(sender, socketAsyncEvent);
                }
                socketEvents.Add(e);
            }
            else
            {
                acceptedSocket.Dispose();
            }
            var listenSocket = e.UserToken as System.Net.Sockets.Socket;
            if (!listenSocket.AcceptAsync(e))
            {
                OnSocketAccepted(sender, e);
            }
        }

        private void OnSocketCompleted(object sender, SocketAsyncEventArgs e)
        {
            var socket = e.UserToken as System.Net.Sockets.Socket;
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
