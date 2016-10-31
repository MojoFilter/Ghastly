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
                    case CommandCode.GetSceneImage:
                        await this.HandleGetSceneImage(args.Socket.InputStream, args.Socket.OutputStream);
                        break;
                    case CommandCode.PlayInterval:
                        await this.HandlePlayInterval(args.Socket.InputStream);
                        break;
                }
                args.Socket.Dispose();
            }
            finally { }
        }

        private async Task HandlePlayInterval(IInputStream inputStream)
        {
            using (var reader = new DataReader(inputStream))
            {
                var sceneId = await this.ReadSceneId(reader);
                await reader.LoadAsync(8);
                var seconds = reader.ReadDouble();
                await this.ghast.PlayInterval(sceneId, TimeSpan.FromSeconds(seconds));
            }
        }

        private async Task HandleGetSceneImage(IInputStream inputStream, IOutputStream outputStream)
        {
            int sceneId;
            using (var reader = new DataReader(inputStream))
            {
                sceneId = await ReadSceneId(reader);
            }
            var image = await this.ghast.GetSceneImage(sceneId);
            using (var writer = new DataWriter(outputStream))
            {
                writer.WriteUInt32((uint)image.Length);
                writer.WriteBytes(image);
                await writer.StoreAsync();
                await writer.FlushAsync();
            }
        }

        private async Task HandleBeginScene(IInputStream stream)
        {
            using (var reader = new DataReader(stream))
            {
                var sceneId = await ReadSceneId(reader);
                reader.DetachStream();
                await this.ghast.BeginScene(sceneId);
            }
        }

        private async Task<int> ReadSceneId(DataReader reader)
        {
            await reader.LoadAsync(4);
            return reader.ReadInt32();
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
