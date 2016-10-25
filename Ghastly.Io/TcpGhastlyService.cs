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

        public IObservable<SceneDescription> GetScenes() => Observable.Create<SceneDescription>(async obs =>
        {
            var socket = new StreamSocket();
            await socket.ConnectAsync(this.host, this.port);
            using (var writer = new DataWriter(socket.OutputStream))
            {
                writer.WriteByte((byte)CommandCode.GetScenes);
                writer.DetachStream();
            }

            var inputBuffer = new Buffer(1);
            var buffer = await socket.InputStream.ReadAsync(inputBuffer, 1, InputStreamOptions.None);
            var reader = DataReader.FromBuffer(buffer);
            var length = reader.ReadUInt32();

            inputBuffer = new Buffer(length);
            buffer = await socket.InputStream.ReadAsync(inputBuffer, length, InputStreamOptions.None);
            reader = DataReader.FromBuffer(buffer);
            var data = reader.ReadString(length);
            var scenes = JsonConvert.DeserializeObject<IEnumerable<SceneDescription>>(data);
            return scenes.ToObservable().Subscribe(obs);
        });
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

        public async Task Listen() => await this.listener.BindServiceNameAsync(this.port.ToString());

        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var socket = args.Socket;
            var inputBuffer = new Buffer(1);
            try
            {
                var buffer = await socket.InputStream.ReadAsync(inputBuffer, 1, InputStreamOptions.None);
                var reader = DataReader.FromBuffer(buffer);
                IAsyncOperation<uint> taskLoad = reader.LoadAsync(1);
                taskLoad.AsTask().Wait();
                uint bytesRead = taskLoad.GetResults();
                var command = (CommandCode)reader.ReadByte();
                switch (command)
                {
                    case CommandCode.GetScenes:
                        await this.HandleGetScenes(socket.OutputStream);
                        break;
                }
                socket.Dispose();
            }
            finally { }
        }

        private async Task HandleGetScenes(IOutputStream outputStream)
        {
            var scenes = this.ghast.GetScenes().ToEnumerable();
            var data = JsonConvert.SerializeObject(scenes);
            using (var writer = new DataWriter(outputStream))
            {
                writer.WriteUInt32(writer.MeasureString(data));
                writer.WriteString(data);
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
