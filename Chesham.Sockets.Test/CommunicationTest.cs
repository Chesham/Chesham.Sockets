using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chesham.Sockets.Test
{
    [TestClass]
    public class CommunicationTest : TestContext
    {
        [TestMethod]
        public async Task TestServerToClient()
        {
            var endPoint = randomIPEndPoint;
            var server = new SocketServer();
            server.OnEvent += (_, e_) =>
            {
                if (e_ is OnSocketAccepted)
                {
                    var e = e_ as OnSocketAccepted;
                    var connection = e.connection;
                    connection.SendAsync(payloadBytes)
                        .AsTask()
                        .ContinueWith(_ => connection.Close());
                    e.isAccept = true;
                }
            };
            server.Listen(endPoint);

            var receivedTcs = new TaskCompletionSource<byte[]>();
            var client = new SocketConnection();
            client.OnEvent += (_, e_) =>
            {
                if (e_ is OnSocketReceived)
                {
                    var e = e_ as OnSocketReceived;
                    receivedTcs.SetResult(e.buffer);
                }
            };
            await client.ConnectAsync(endPoint);
            Assert.IsTrue(payloadBytes.SequenceEqual(await receivedTcs.Task));
        }

        [TestMethod]
        public async Task TestClientToServer()
        {
            var endPoint = randomIPEndPoint;
            var receivedTcs = new TaskCompletionSource<byte[]>();
            var connections = new List<SocketConnection>();
            var server = new SocketServer();
            server.OnEvent += (_, e_) =>
            {
                if (e_ is OnSocketAccepted)
                {
                    var e = e_ as OnSocketAccepted;
                    var connection = e.connection;
                    connection.OnEvent += (_, e_) =>
                    {
                        if (e_ is OnSocketReceived)
                        {
                            var e = e_ as OnSocketReceived;
                            receivedTcs.SetResult(e.buffer);
                        }
                    };
                    connections.Add(connection);
                    e.isAccept = true;
                }
            };
            server.Listen(endPoint);

            var client = new SocketConnection();
            await client.ConnectAsync(endPoint);
            await client.SendAsync(payloadBytes);
            Assert.IsTrue(payloadBytes.SequenceEqual(await receivedTcs.Task));
        }
    }
}
