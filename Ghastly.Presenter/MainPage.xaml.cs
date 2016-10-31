using Ghastly.Io;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
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
            this.ended = Observable.FromEventPattern(player.MediaPlayer, "MediaEnded").SubscribeOnDispatcher().Publish().RefCount();
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
            var service = new GhastlyService(folder);
            this.listener = new TcpGhastlyServiceListener(service);
            service.StartScene
                .OfType<SceneDescription>()
                .Select(scene => scene.Idle)
                .SelectMany(file => this.PlayLoop(file, folder))
                .Subscribe();

            service.StartScene
                .OfType<SceneDescription>()
                .SelectMany(scene => service.TriggerScene.Select(_ => scene))
                .SelectMany(scene => this.PlayOnce(scene.Active, scene.Idle, folder))
                .Subscribe();


            service.PlayIntervalSource
                .OfType<Tuple<SceneDescription, TimeSpan>>()
                .Subscribe(async p =>
                {
                    var loop = await this.PlayInterval(p.Item2, p.Item1.Active, p.Item1.Idle, folder);
                    service.StartScene.OfType<SceneDescription>().Select(_ => Unit.Default)
                    .Merge(service.TriggerScene).Subscribe(_ => loop.Dispose());
                });

            await this.listener.Listen();

            int currentSceneId = 0;
            service.StartScene.OfType<SceneDescription>().Select(scene => scene.Id).Subscribe(id => currentSceneId = id);
        }

        /// <summary>
        /// Loads the byte data from a StorageFile
        /// </summary>
        /// <param name="file">The file to read</param>
        public static async Task<byte[]> ReadFile(StorageFile file)
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
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.player.MediaPlayer.IsLoopingEnabled = true);
            var file = await folder.GetFileAsync(fileName);
            var data = await ReadFile(file);
            var stream = new MemoryStream(data).AsRandomAccessStream();
            var source = MediaSource.CreateFromStream(stream, file.ContentType);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.player.MediaPlayer.Source = source);
            return Unit.Default;
        }

        string currentFile;
        private IObservable<EventPattern<object>> ended;

        private async Task<Task<Unit>> PlayOnce(string fileName, string idleFileName, StorageFolder folder)
        {
            if (fileName == currentFile)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.player.MediaPlayer.PlaybackSession.Position = TimeSpan.Zero);
            }
            else
            {
                currentFile = fileName;
                var file = await folder.GetFileAsync(fileName);
                var source = MediaSource.CreateFromStorageFile(file);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.player.MediaPlayer.IsLoopingEnabled = false;
                    this.player.MediaPlayer.Source = source;
                });
            }
            var ended = this.ended.Take(1).Select(_ => Unit.Default);
            ended.SelectMany(_ => this.PlayLoop(idleFileName, folder)).Subscribe();
            return ended.ToTask();
        }

        private async Task<IDisposable> PlayInterval(TimeSpan interval, string fileName, string idleFileName, StorageFolder folder)
        {
            var loopingDisposable = new BooleanDisposable();
            Func<bool> looping = () => !loopingDisposable.IsDisposed;
            await this.PlayLoop(idleFileName, folder);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                await Task.Delay(interval);
                while (looping())
                {
                    var playtime = await this.PlayOnce(fileName, idleFileName, folder);
                    await playtime;
                    await Task.Delay(interval);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return loopingDisposable;
        }

        class GhastlyService : IGhastlyService
        {
            public GhastlyService(StorageFolder imageFolder)
            {
                this.imageFolder = imageFolder;
                this.StartScene.Select(_ => Unit.Default)
                    .Merge(this.TriggerScene)
                    .Subscribe(_CancelInterval);
            }

            private SceneDescription[] scenes = new[]
            {
                new SceneDescription()
                {
                    Id = 0,
                    Name = "Boneyard Band (Black Background)",
                    Idle = "BC_Buffer_Curtain_Win_Black_H.mp4",
                    Active = "BC_BoneyardBand_Win_Black_H.mp4",
                    Image = "BC_BoneyardBand_Win_Black_H.png"
                },
                new SceneDescription()
                {
                    Id = 1,
                    Name = "Bleeding Wall (Spotlight)",
                    Idle ="BW_Buffer_Wall_Spotlight_H.mp4",
                    Active = "BW_DrippingBlood_Wall_Spotlight_H.mp4",
                    Image = "BW_DrippingBlood_Wall_Spotlight_H.png"
                },
                new SceneDescription()
                {
                    Id = 2,
                    Name = "He's Alive",
                    Idle = "TnT_Buffer_Black_H.mp4",
                    Active = "TnT_HesAlive_Win_H.mp4",
                    Image = "TnT_HesAlive_Win_H.png"
                },
                new SceneDescription()
                {
                    Id = 3,
                    Name = "Treat Thief",
                    Idle = "TnT_Buffer_Black_H.mp4",
                    Active = "TnT_TreatThief_Win_H.mp4",
                    Image = "TnT_TreatThief_Win_H.png"
                },
                new SceneDescription()
                {
                    Id = 4,
                    Name = "Boneyard Band (Shadow)",
                    Idle = "BC_Buffer_Curtain_Win_Shad_H.mp4",
                    Active = "BC_BoneyardBand_Win_Shad_H.mp4",
                    Image = "BC_BoneyardBand_Win_Shad_H.png"
                },
                new SceneDescription()
                {
                    Id = 5,
                    Name = "Bleeding Wall (Lightning)",
                    Idle = "BW_Buffer_Wall_Lightning_H.mp4",
                    Active = "BW_DrippingBlood_Wall_Lightning_H.mp4",
                    Image = "BW_Wall_Lightning_H.png"
                },
                new SceneDescription()
                {
                    Id = 6,
                    Name = "Bleeding Window",
                    Idle = "BW_Buffer_Win_H.mp4",
                    Active = "BW_DrippingBlood_Win_H.mp4",
                    Image = "BW_DrippingBlood_Win_H.png"
                }

            };

            private SceneDescription GetScene(int sceneId) => this.scenes.FirstOrDefault(s => s.Id == sceneId);

            public Task<IEnumerable<SceneDescription>> GetScenes() => Task.FromResult(scenes.AsEnumerable());

            public Task ActivateScene() => Task.Run(() => this._TriggerScene.OnNext(Unit.Default));

            public Task<int> GetCurrentSceneId() => Task.FromResult(_StartScene.Value.Id);

            public Task BeginScene(int sceneId) => Task.Run(() => this.StartScene.OnNext(GetScene(sceneId)));

            public async Task<byte[]> GetSceneImage(int sceneId) =>
                await MainPage.ReadFile(await this.imageFolder.GetFileAsync(GetScene(sceneId).Image));

            public Task PlayInterval(int sceneId, TimeSpan interval) => Task.Run(() => PlayIntervalSource.OnNext(Tuple.Create(GetScene(sceneId), interval)));

            private BehaviorSubject<SceneDescription> _StartScene = new BehaviorSubject<SceneDescription>(null);
            public ISubject<SceneDescription> StartScene => _StartScene;

            private Subject<Unit> _CancelInterval = new Subject<Unit>();
            private IObservable<Unit> CancelInterval => this._CancelInterval.Repeat();

            private Subject<Unit> _TriggerScene = new Subject<Unit>();
            private StorageFolder imageFolder;

            public ISubject<Unit> TriggerScene => _TriggerScene;

            public ISubject<Tuple<SceneDescription, TimeSpan>> PlayIntervalSource { get; } = new Subject<Tuple<SceneDescription, TimeSpan>>();
        }
    }
}
