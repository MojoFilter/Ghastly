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
        private readonly HostName host;
        private readonly string port;

        public TcpGhastlyClient(string host, int port)
        {
            this.host = new HostName(host);
            this.port = port.ToString();
        }

        public async Task ActivateScene() =>
            (await this.SendCommand(CommandCode.ActivateScene)).Dispose();

        public async Task BeginScene(int sceneId)
        {
            using (var socket = await this.SendCommand(CommandCode.BeginScene))
            {
                await WriteSceneId(sceneId, socket.OutputStream);
            }
        }

        private async Task WriteSceneId(int sceneId, IOutputStream stream)
        {
            using (var writer = new DataWriter(stream))
            {
                writer.WriteInt32(sceneId);
                await writer.StoreAsync();
                writer.DetachStream();
            }
        }

        public async Task<int> GetCurrentSceneId()
        {
            using (var socket = await this.SendCommand(CommandCode.GetCurrentSceneId))
            using (var reader = new DataReader(socket.InputStream))
            {
                await reader.LoadAsync(1);
                return (int)reader.ReadByte();
            }
        }

        public async Task<byte[]> GetSceneImage(int sceneId)
        {
            using (var socket = await this.SendCommand(CommandCode.GetSceneImage))
            using (var reader = new DataReader(socket.InputStream))
            {
                await WriteSceneId(sceneId, socket.OutputStream);
                await reader.LoadAsync(4);
                var length = reader.ReadUInt32();
                await reader.LoadAsync(length);
                return reader.ReadBuffer(length).ToArray();
            }
        }

        public async Task<IEnumerable<SceneDescription>> GetScenes() 
        {
            using (var socket = await this.SendCommand(CommandCode.GetScenes))
            using (var reader = new DataReader(socket.InputStream))
            {
                reader.InputStreamOptions = InputStreamOptions.Partial;
                await reader.LoadAsync(1024);
                var data = reader.ReadString(reader.UnconsumedBufferLength);
                return JsonConvert.DeserializeObject<IEnumerable<SceneDescription>>(data);
            }
        }

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
        GetSceneImage
    }
}
