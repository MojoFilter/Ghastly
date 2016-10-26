using Ghastly.Io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.listener = new TcpGhastlyServiceListener(new GhastlyService());
            await this.listener.Listen();
        }

        class GhastlyService : IGhastlyService
        {
            //public IObservable<SceneDescription> GetScenes() => Observable.Create<SceneDescription>(obs => () =>
            //{
            //    obs.OnNext(new SceneDescription() { Name = "A Skeleton Band" });
            //    obs.OnNext(new SceneDescription() { Name = "Menstrual Walls" });
            //    obs.OnCompleted();
            //});
            public Task<IEnumerable<SceneDescription>> GetScenes() => Task.FromResult(new[]
            {
                new SceneDescription() { Name = "A Skeleton Band" },
                new SceneDescription() { Name = "Menstrual Walls" }
            }.AsEnumerable());
        }
    }
}
