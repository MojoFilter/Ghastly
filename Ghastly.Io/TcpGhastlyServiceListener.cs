using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Ghastly.Io
{
    public class TcpGhastlyServiceListener
    {
        private readonly StreamSocketListener listener;
        private readonly int port;
        private readonly IGhastlyService ghast;

        public TcpGhastlyServiceListener(IGhastlyService ghast, int port = TcpGhastlyClient.DefaultPort)
        {
            this.port = port;
            this.ghast = ghast;
            this.listener = new StreamSocketListener();
            this.listener.ConnectionReceived += Listener_ConnectionReceived;
        }

        public async Task Listen()
        {
            await this.listener.BindServiceNameAsync(this.port.ToString());
        }

        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                var command = await this.ReadCommand(args.Socket);
                switch (command)
                {
                    case CommandCode.GetScenes:
                        await this.HandleGetScenes(args.Socket.OutputStream);
                        break;
                    case CommandCode.ActivateScene:
                        await this.HandleActivateScene();
                        break;
                    case CommandCode.BeginScene:
                        await this.HandleBeginScene(args.Socket.InputStream);
                        break;
                }
                args.Socket.Dispose();
            }
            finally { }
        }

        private async Task HandleBeginScene(IInputStream stream)
        {
            using (var reader = new DataReader(stream))
            {
                await reader.LoadAsync(4);
                var sceneId = reader.ReadInt32();
                reader.DetachStream();
                await this.ghast.BeginScene(sceneId);
            }
        }

        private Task HandleActivateScene() => this.ghast.ActivateScene();

        private async Task<byte> ReadByte(IInputStream stream)
        {
            using (var reader = new DataReader(stream))
            {
                await reader.LoadAsync(1);
                try
                {
                    return reader.ReadByte();
                }
                finally
                {
                    reader.DetachStream();
                }
            }
        }

        private async Task<CommandCode> ReadCommand(StreamSocket socket) =>
            (CommandCode)(await ReadByte(socket.InputStream));

        private async Task HandleGetScenes(IOutputStream outputStream)
        {
            var scenes = await this.ghast.GetScenes();
            var data = JsonConvert.SerializeObject(scenes);
            using (var writer = new DataWriter(outputStream))
            {
                writer.WriteBytes(Encoding.UTF8.GetBytes(data));
                await writer.StoreAsync();
                await writer.FlushAsync();
                writer.DetachStream();
            }
        }
    }
}
