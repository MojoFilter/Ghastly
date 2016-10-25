using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Reactive.Disposables;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace Ghastly.Io
{
    public class TcpCommandListener
    {
        private ISubject<string> subject = new Subject<string>();
        public IObservable<string> Messages => this.subject.AsObservable();

        public async Task Listen()
        {
            var listener = new StreamSocketListener();
            listener.ConnectionReceived += Listener_ConnectionReceived;
            await listener.BindServiceNameAsync("11337");
            
        }

        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var socket = args.Socket;
            var inputBuffer = new Windows.Storage.Streams.Buffer(1024);
            while (true)
            {
                var buffer = await socket.InputStream.ReadAsync(inputBuffer, inputBuffer.Capacity, InputStreamOptions.Partial);
                var reader = DataReader.FromBuffer(buffer);
                this.subject.OnNext(reader.ReadString(buffer.Length));
            }
        }
        
    }
}
