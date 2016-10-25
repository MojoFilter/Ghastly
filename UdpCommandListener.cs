using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Reactive.Disposables;

namespace Ghastly.Io
{
    public class UdpCommandListener
    {
        public IObservable<string> Listen()
        {
            var c = new TcpListener(IPAddress.Any, 1337);
            c.Start();
            var cancel = false;
            return Observable.Create<string>(obs => 
            {
                Task.Run(async () =>
                {
                    while (!cancel)
                    {
                        this.DealWithClient(await c.AcceptTcpClientAsync(), obs);
                    }
                });
                return Disposable.Create(() => cancel = true);
            });
        }

        private async Task DealWithClient(TcpClient client, IObserver<string> obs)
        {
            var stream = client.GetStream();
            var buffer = new byte[4];
            while (stream.CanRead)
            {
                int count = await stream.ReadAsync(buffer, 0, buffer.Length);
                obs.OnNext(Encoding.UTF8.GetString(buffer, 0, count));
            }
        }
    }
}
