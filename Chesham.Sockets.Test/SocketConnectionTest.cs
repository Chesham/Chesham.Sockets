using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Chesham.Sockets.Test
{
    [TestClass]
    public class SocketConnectionTest : TestContext
    {
        [TestMethod]
        public async Task TestConnect()
        {
            var endPoint = randomIPEndPoint;
            var listener = new TcpListener(endPoint);
            listener.Start();
            var socketConnection = new SocketConnection();
            await socketConnection.ConnectAsync(endPoint);
        }

        [TestMethod]
        public async Task TestSend()
        {
            var endPoint = randomIPEndPoint;
            var listener = new TcpListener(endPoint);
            listener.Start();
            var socketConnection = new SocketConnection();
            await socketConnection.ConnectAsync(endPoint);
            using (var client = await listener.AcceptTcpClientAsync())
            {
                await socketConnection.SendAsync(payloadBytes);
                var receiveBuffer = new byte[payloadBytes.Length];
                await client.GetStream().ReadAsync(receiveBuffer);
                Assert.IsTrue(receiveBuffer.SequenceEqual(payloadBytes));
            }
        }

        [TestMethod]
        public async Task TestSendMultiple()
        {
            var endPoint = randomIPEndPoint;
            var listener = new TcpListener(endPoint);
            listener.Start();
            var socketConnection = new SocketConnection();
            await socketConnection.ConnectAsync(endPoint);
            using (var client = await listener.AcceptTcpClientAsync())
            {
                var testTimes = 10;
                for (var i = 0; i < testTimes; ++i)
                {
                    await socketConnection.SendAsync(payloadBytes);
                    var receiveBuffer = new byte[payloadBytes.Length];
                    await client.GetStream().ReadAsync(receiveBuffer);
                    Assert.IsTrue(receiveBuffer.SequenceEqual(payloadBytes));
                }
            }
        }
    }
}
