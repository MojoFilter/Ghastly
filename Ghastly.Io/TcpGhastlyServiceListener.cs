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

        public TcpGhastlyServiceListener(IGhastlyService ghast, int port = TcpGhastlyService.DefaultPort)
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
                }
                args.Socket.Dispose();
            }
            finally { }
        }

        private Task HandleActivateScene() => this.ghast.ActivateScene();

        private async Task<CommandCode> ReadCommand(StreamSocket socket)
        {
            using (var reader = new DataReader(socket.InputStream))
            {
                await reader.LoadAsync(1);
                return (CommandCode)reader.ReadByte();
            }

        }

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
