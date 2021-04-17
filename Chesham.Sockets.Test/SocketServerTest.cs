using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
            var cts = new CancellationTokenSource();
            var totalClientCount = 100;
            var receiveBuffers = new List<byte[]>();
            server.OnEvent += (_, e_) =>
            {
                if (e_ is OnSocketAccepted)
                {
                    var e = e_ as OnSocketAccepted;
                    e.isAccept = true;
                }
                else if (e_ is OnSocketReceived)
                {
                    var e = e_ as OnSocketReceived;
                    lock (receiveBuffers)
                    {
                        receiveBuffers.Add(e.buffer);
                        if (receiveBuffers.Count() == totalClientCount)
                        {
                            cts.Cancel();
                        }
                    }
                }
            };
            var endPoint = randomIPEndPoint;
            server.Listen(endPoint, cts.Token);
            var clients = Enumerable.Range(0, totalClientCount)
                .Select(i => new TcpClient(endPoint.Address.ToString(), endPoint.Port))
                .Select(async client =>
                {
                    using (client)
                    {
                        await client.GetStream().WriteAsync(payloadBytes, cts.Token);
                    }
                })
                .ToArray();
            Task.WaitAll(clients);
            SpinWait.SpinUntil(() => cts.IsCancellationRequested);
            Assert.AreEqual(totalClientCount, receiveBuffers.Count());
            foreach (var receiveBuffer in receiveBuffers)
            {
                Assert.IsTrue(payloadBytes.SequenceEqual(receiveBuffer));
            }
        }

        [TestMethod]
        public async Task TestSend()
        {
            var socketTask = new TaskCompletionSource<System.Net.Sockets.Socket>();
            var server = new SocketServer();
            server.OnEvent += (_, e_) =>
            {
                if (e_ is OnSocketAccepted)
                {
                    var e = e_ as OnSocketAccepted;
                    e.isAccept = true;
                    socketTask.SetResult(e.socket);
                }
            };
            var cts = new CancellationTokenSource();
            var endPoint = randomIPEndPoint;
            server.Listen(endPoint, cts.Token);
            using (var client = new TcpClient(endPoint.Address.ToString(), endPoint.Port))
            {
                var socket = await socketTask.Task;
                var receivedBytes = new byte[payloadBytes.Length];
                var readTask = client.GetStream().ReadAsync(receivedBytes);
                Assert.AreEqual(payloadBytes.Length, await socket.SendAsync(payloadBytes, SocketFlags.None));
                Assert.IsTrue(payloadBytes.SequenceEqual(receivedBytes));
            }
        }
    }
}
