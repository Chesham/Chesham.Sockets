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
    public class SocketServerTest
    {
        [TestMethod]
        public void TestListen()
        {
            var server = new SocketServer();
            var cts = new CancellationTokenSource();
            var totalClientCount = 100;
            var buffer = Encoding.UTF8.GetBytes("Hello World");
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
            var endPoint = new IPEndPoint(IPAddress.Any, 80);
            server.Listen(endPoint, cts.Token);
            var clients = Enumerable.Range(0, totalClientCount)
                .Select(i => new TcpClient("127.0.0.1", 80))
                .Select(async client =>
                {
                    using (client)
                    {
                        await client.GetStream().WriteAsync(buffer, cts.Token);
                    }
                })
                .ToArray();
            Task.WaitAll(clients);
            SpinWait.SpinUntil(() => cts.IsCancellationRequested);
            Assert.AreEqual(totalClientCount, receiveBuffers.Count());
            foreach (var receiveBuffer in receiveBuffers)
            {
                Assert.IsTrue(buffer.SequenceEqual(receiveBuffer));
            }
        }
    }
}
