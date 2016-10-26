using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Buffer = Windows.Storage.Streams.Buffer;
using System.Reactive.Linq;
using Newtonsoft.Json;
using Windows.Networking;
using Windows.Foundation;
using Windows.ApplicationModel.Background;

namespace Ghastly.Io
{
    public class TcpGhastlyService : IGhastlyService
    {
        public const int DefaultPort = 11337;
        private readonly HostName host;
        private readonly string port;

        public TcpGhastlyService(string host, int port)
        {
            this.host = new HostName(host);
            this.port = port.ToString();
        }

        public async Task<IEnumerable<SceneDescription>> GetScenes() 
        {
            var socket = new StreamSocket();
            await socket.ConnectAsync(this.host, this.port);
            using (var writer = new DataWriter(socket.OutputStream))
            {
                writer.WriteByte((byte)CommandCode.GetScenes);
                await writer.StoreAsync();
                await writer.FlushAsync();
                writer.DetachStream();
            }

            using (var reader = new DataReader(socket.InputStream))
            {
                reader.InputStreamOptions = InputStreamOptions.Partial;
                await reader.LoadAsync(1024);
                var data = reader.ReadString(reader.UnconsumedBufferLength);
                return JsonConvert.DeserializeObject<IEnumerable<SceneDescription>>(data);
            }
        }
    }

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
                }
                args.Socket.Dispose();
            }
            finally { }
        }

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

    public enum CommandCode : byte
    {
        GetScenes
    }
}
