using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Chesham.Sockets.Test
{
    [TestClass]
    public class SocketServerTest : TestContext
    {
        [TestMethod]
        public void TestListen()
        {
            var server = new SocketServer();
            var totalClientCount = 100;
            var receiveBuffers = new List<byte[]>();
            var connections = new List<SocketConnection>();
            server.OnEvent += (_, e_) =>
            {
                if (e_ is OnSocketAccepted)
                {
                    var e = e_ as OnSocketAccepted;
                    e.connection.OnEvent += (_, e_) =>
                    {
                        if (e_ is OnSocketReceived)
                        {
                            var e = e_ as OnSocketReceived;
                            lock (receiveBuffers)
                            {
                                receiveBuffers.Add(e.buffer);
                            }
                        }
                    };
                    connections.Add(e.connection);
                    e.isAccept = true;
                }
            };
            var endPoint = randomIPEndPoint;
            server.Listen(endPoint, default);
            var clients = Enumerable.Range(0, totalClientCount)
                .Select(i => new TcpClient(endPoint.Address.ToString(), endPoint.Port))
                .Select(async client =>
                {
                    using (client)
                    {
                        await client.GetStream().WriteAsync(payloadBytes, default);
                    }
                })
                .ToArray();
            Task.WaitAll(clients);
            SpinWait.SpinUntil(() => receiveBuffers.Count() == clients.Count());
            Assert.AreEqual(totalClientCount, receiveBuffers.Count());
            foreach (var receiveBuffer in receiveBuffers)
            {
                Assert.IsTrue(payloadBytes.SequenceEqual(receiveBuffer));
            }
        }

        [TestMethod]
        public async Task TestSend()
        {
            var connectionTask = new TaskCompletionSource<SocketConnection>();
            var server = new SocketServer();
            server.OnEvent += (_, e_) =>
            {
                if (e_ is OnSocketAccepted)
                {
                    var e = e_ as OnSocketAccepted;
                    e.isAccept = true;
                    connectionTask.SetResult(e.connection);
                }
            };
            var endPoint = randomIPEndPoint;
            server.Listen(endPoint, default);
            using (var client = new TcpClient(endPoint.Address.ToString(), endPoint.Port))
            {
                var connection = await connectionTask.Task;
                var receivedBytes = new byte[payloadBytes.Length];
                var readTask = client.GetStream().ReadAsync(receivedBytes);
                Assert.AreEqual(payloadBytes.Length, await connection.SendAsync(payloadBytes));
                Assert.IsTrue(payloadBytes.SequenceEqual(receivedBytes));
            }
        }
    }
}
