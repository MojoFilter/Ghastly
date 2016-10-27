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

        public async Task ActivateScene() =>
            (await this.SendCommand(CommandCode.ActivateScene)).Dispose();

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
        ActivateScene
    }
}
