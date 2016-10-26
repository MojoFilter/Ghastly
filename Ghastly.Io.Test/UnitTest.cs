using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Ghastly.Io;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

namespace Ghastly.Io.Test
{
    [TestClass]
    public class TcpGahstlyServiceListenerTests
    {
        string host = "192.168.0.19";
        int port = 11338;

        [TestMethod]
        public async Task ListenAcceptsConnection()
        {
            var service = new StubIGhastlyService();
            var listener = new TcpGhastlyServiceListener(service, port);
            await listener.Listen();
            using (var socket = await ConnectSocket())
            {
            }
        }

        [TestMethod]
        public async Task GetScenesInvoked()
        {
            var called = false;
            var service = new StubIGhastlyService()
                .GetScenes(() =>
                {
                    called = true;
                    return Task.FromResult(Enumerable.Empty<SceneDescription>());
                });
            var listener = new TcpGhastlyServiceListener(service, port);
            await listener.Listen();
            using (var socket = await ConnectSocket())
            {
                await socket.SendAsync(CommandCode.GetScenes);
                await Task.Delay(1000);
            }
            Assert.IsTrue(called, "Handler was not called");
        }

        [TestMethod]
        public async Task GetScenesEmpty() => await AssertGetScenesResultMatches();

        private async Task AssertGetScenesResultMatches(params SceneDescription[] results)
        {
            var expectedResult = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(results));

            var service = new StubIGhastlyService()
                .GetScenes(() => Task.FromResult(results.AsEnumerable()));
            var listener = new TcpGhastlyServiceListener(service, port);
            await listener.Listen();
            using (var socket = await ConnectSocket())
            {
                await socket.SendAsync(CommandCode.GetScenes);
                uint length = await socket.ReceiveUint32();
                byte[] result = await socket.ReceiveBytes(length);
                CollectionAssert.AreEqual(result, expectedResult);
            }
        }

        private async Task<SocketAdapter> ConnectSocket() => await SocketAdapter.Connect(host, port);

        class SocketAdapter : IDisposable
        {
            private readonly Socket socket;

            public SocketAdapter(Socket socket)
            {
                this.socket = socket;
            }

            public void Dispose()
            {
                this.socket.Dispose();
            }

            public static async Task<SocketAdapter> Connect(string host, int port)
            {
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(host, port);
                return new SocketAdapter(socket);
            }

            internal async Task SendAsync(CommandCode command) =>
                await this.socket.SendAsync(new ArraySegment<byte>(new[] { (byte)command }), SocketFlags.None);

            internal async Task<uint> ReceiveUint32() => BitConverter.ToUInt32(await ReceiveBytes(4), 0);

            internal async Task<byte[]> ReceiveBytes(uint length)
            {
                var buffer = new ArraySegment<Byte>(new byte[4]);
                await socket.ReceiveAsync(buffer, SocketFlags.None);
                return buffer.Array;
            }
        }
    }
}
