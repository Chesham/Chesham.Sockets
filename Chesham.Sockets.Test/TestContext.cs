using System;
using System.Net;
using System.Text;

namespace Chesham.Sockets.Test
{
    public class TestContext
    {
        protected static Random random { get; } = new Random((int)DateTime.UtcNow.Ticks);

        protected static int lowerPort { get; } = 50000;

        protected static int upperPort { get; } = 60000;

        protected static IPAddress ipAddress { get; } = IPAddress.Loopback;

        protected static string payloadString { get; } = "Hello World";

        protected static byte[] payloadBytes { get; } = Encoding.UTF8.GetBytes(payloadString);

        protected int nextPort => random.Next(lowerPort, upperPort);

        protected IPEndPoint randomIPEndPoint => new IPEndPoint(ipAddress, nextPort);
    }
}
