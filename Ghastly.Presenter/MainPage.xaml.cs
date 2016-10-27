using Ghastly.Io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Ghastly.Presenter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private TcpGhastlyServiceListener listener;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }



        public string VideoUri
        {
            get { return (string)GetValue(VideoUriProperty); }
            set { SetValue(VideoUriProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VideoUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoUriProperty =
            DependencyProperty.Register("VideoUri", typeof(string), typeof(MainPage), new PropertyMetadata(@"‪C:\Users\Joshua\Videos\Projection\Psychedelic\Psychedelic Cartoon Visuals.mp4"));



        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await KnownFolders.GetFolderForUserAsync(null /* current user */, KnownFolderId.CameraRoll);
            var service = new GhastlyService();
            this.listener = new TcpGhastlyServiceListener(service);
            service.StartScene
                .Select(scene => scene.Idle)
                .ObserveOnDispatcher()
                .SelectMany(file => this.PlayLoop(file, folder))
                .Subscribe();

            service.StartScene
                .SelectMany(scene => service.TriggerScene.Select(_ => scene))
                .ObserveOnDispatcher()
                .SelectMany(scene => this.PlayOnce(scene.Active, scene.Idle, folder))
                .Subscribe();

            this.player.IsFullWindow = true;

            await this.listener.Listen();
            service.StartScene.OnNext((await service.GetScenes()).Skip(1).First());

            int currentSceneId = 0;
            service.StartScene.Select(scene => scene.Id).Subscribe(id => currentSceneId = id);
            this.Tapped += (s, ee) => service.ActivateScene();

        }

        /// <summary>
        /// Loads the byte data from a StorageFile
        /// </summary>
        /// <param name="file">The file to read</param>
        public async Task<byte[]> ReadFile(StorageFile file)
        {
            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            return fileBytes;
        }

        private async Task<Unit> PlayLoop(string fileName, StorageFolder folder)
        {
            this.player.MediaPlayer.IsLoopingEnabled = true;
            var file = await folder.GetFileAsync(fileName);
            var data = await this.ReadFile(file);
            var stream = new MemoryStream(data).AsRandomAccessStream();
            this.player.MediaPlayer.Source = MediaSource.CreateFromStream(stream, file.ContentType);
            return Unit.Default;
        }

        private async Task<Unit> PlayOnce(string fileName, string idleFileName, StorageFolder folder)
        {
            this.player.MediaPlayer.IsLoopingEnabled = false;
            this.player.MediaPlayer.Source = MediaSource.CreateFromStorageFile(await folder.GetFileAsync(fileName));
            Observable.FromEventPattern(this.player.MediaPlayer, "MediaEnded")
                .Take(1)
                .ObserveOnDispatcher()
                .SelectMany(_ => this.PlayLoop(idleFileName, folder))
                .Subscribe();
            return Unit.Default;
        }

        class GhastlyService : IGhastlyService
        {
            public GhastlyService()
            {

            }

            private SceneDescription[] scenes = new[]
            {
                new SceneDescription()
                {
                    Id = 0,
                    Name = "Boneyard Band Black Background",
                    Idle = "BC_Buffer_Curtain_Win_Black_H.mp4",
                    Active = "BC_BoneyardBand_Win_Black_H.mp4"
                },
                new SceneDescription()
                {
                    Id = 1,
                    Name = "Bleeding Wall Spotlight",
                    Idle ="BW_Buffer_Wall_Spotlight_H.mp4",
                    Active = "BW_DrippingBlood_Wall_Spotlight_H.mp4"
                }
            };

            public Task<IEnumerable<SceneDescription>> GetScenes() => Task.FromResult(scenes.AsEnumerable());

            public Task ActivateScene() => Task.Run(() => this._TriggerScene.OnNext(Unit.Default));

            public Task<int> GetCurrentSceneId() => Task.FromResult(_StartScene.Value.Id);

            public Task BeginScene(int sceneId) => Task.Run(() => this.StartScene.OnNext(this.scenes.FirstOrDefault(s => s.Id == sceneId)));

            private BehaviorSubject<SceneDescription> _StartScene = new BehaviorSubject<SceneDescription>(null);
            public ISubject<SceneDescription> StartScene => _StartScene;

            private Subject<Unit> _TriggerScene = new Subject<Unit>();
            public ISubject<Unit> TriggerScene => _TriggerScene;
        }
    }
}
