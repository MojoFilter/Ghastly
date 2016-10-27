using Ghastly.Io;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Net.Sockets;
using Windows.Networking.Sockets;
using Windows.Networking;
using System.Linq;
using System.Collections.Generic;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UapVideoTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string[] sources = new[] {
            "BW_Buffer_Wall_Spotlight_H.mp4",
            "BW_DrippingBlood_Wall_Spotlight_H.mp4"
        };
        private string host = "localhost";
        private int port = 11337;
        private TcpGhastlyServiceListener listener;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await StartBg();
            this.listener = new TcpGhastlyServiceListener(new GhastServer(), this.port);
            await this.listener.Listen();
        }
        

        private async Task StartBg()
        {
            var file = await this.GetVideoFile(sources[0]);
            this.bufferPlayer.SetSource(await file.OpenReadAsync(), file.ContentType);
        }

        private async Task StartImmediate()
        {
            var file = await this.GetVideoFile(sources[1]);
            this.immediatePlayer.SetSource(await file.OpenReadAsync(), file.ContentType);
        }

        private async Task<StorageFile> GetVideoFile(string fileName) => 
            await Package.Current.InstalledLocation.GetFileAsync(fileName);

        private async void Button_Click(object sender, RoutedEventArgs e) => await this.StartImmediate();

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            IGhastlyService client = new TcpGhastlyClient(this.host, this.port);
            var scenes = (await client.GetScenes()).ToList();
        }

        class GhastServer : IGhastlyService
        {
            public Task ActivateScene()
            {
                throw new NotImplementedException();
            }

            public Task BeginScene(int sceneId)
            {
                throw new NotImplementedException();
            }

            public Task<int> GetCurrentSceneId()
            {
                throw new NotImplementedException();
            }

            Task<IEnumerable<SceneDescription>> IGhastlyService.GetScenes() => Task.FromResult(new[]
            {
                new SceneDescription() { Name="Bleeding Wall" },
                new SceneDescription() { Name="Bone Band" }
            }.AsEnumerable());
            
        }
    }
}
