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
using System.Runtime.InteropServices.WindowsRuntime;

namespace Ghastly.Io
{
    public class TcpGhastlyClient : IGhastlyService
    {
        public const int DefaultPort = 11337;
        private HostName host;
        private readonly string port;

        public TcpGhastlyClient(string host, int port)
        {
            this.host = new HostName(host);
            this.port = port.ToString();
        }

        public string Host
        {
            get { return this.host.RawName; }
            set { this.host = new HostName(value); }
        }

        public async Task ActivateScene() =>
            (await this.SendCommand(CommandCode.ActivateScene)).Dispose();

        public async Task BeginScene(int sceneId) =>
            await this.SendCommand(CommandCode.BeginScene, writer => writer.WriteInt32(sceneId));
        
        public async Task PlayInterval(int sceneId, TimeSpan interval) =>
            await this.SendCommand(CommandCode.PlayInterval, writer =>
            {
                writer.WriteInt32(sceneId);
                writer.WriteDouble(interval.TotalSeconds);
            });


        public async Task<int> GetCurrentSceneId() =>
            await this.SendCommandRead(CommandCode.GetCurrentSceneId, async (reader, writer) =>
            {
                await reader.LoadAsync(4);
                return reader.ReadInt32();
            });

        public async Task<byte[]> GetSceneImage(int sceneId) =>
            await this.SendCommandRead(CommandCode.GetSceneImage, async (reader, writer) =>
            {
                writer.WriteInt32(sceneId);
                await writer.StoreAsync();
                await reader.LoadAsync(4);
                var length = reader.ReadUInt32();
                await reader.LoadAsync(length);
                return reader.ReadBuffer(length).ToArray();
            });

        public async Task<IEnumerable<SceneDescription>> GetScenes() =>
            await this.SendCommandRead(CommandCode.GetScenes, async (reader, writer) => {
                reader.InputStreamOptions = InputStreamOptions.Partial;
                await reader.LoadAsync(1024 * 4);
                var data = reader.ReadString(reader.UnconsumedBufferLength);
                return JsonConvert.DeserializeObject<IEnumerable<SceneDescription>>(data);
            });

        private async Task<T> SendCommandRead<T>(CommandCode cmd, Func<DataReader, DataWriter, Task<T>> interact)
        {
            using (var socket = await this.SendCommand(cmd))
            using (var writer = new DataWriter(socket.OutputStream))
            using (var reader = new DataReader(socket.InputStream))
            {
                var retVal = await interact(reader, writer);
                await writer.StoreAsync();
                await writer.FlushAsync();
                return retVal;
            }
        }

        private async Task SendCommand(CommandCode cmd, Action<DataWriter> write) =>
            await this.SendCommandRead(cmd, (reader, writer) =>
            {
                write(writer);
                return Task.FromResult(0);
            });

        private async Task<StreamSocket> SendCommand(CommandCode cmd)
        {
            var socket = new StreamSocket();
            await socket.ConnectAsync(this.host, this.port);
            using (var writer = new DataWriter(socket.OutputStream))
            {
                writer.WriteByte((byte)cmd);
                await writer.StoreAsync();
                writer.DetachStream();
            }
            return socket;
        }
    }

    

    public enum CommandCode : byte
    {
        GetScenes,
        ActivateScene,
        GetCurrentSceneId,
        BeginScene,
        GetSceneImage,
        PlayInterval
    }
}
