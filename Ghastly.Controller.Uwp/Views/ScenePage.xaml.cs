using Ghastly.Controller.Uwp.ViewModels;
using System.Reactive.Linq;
using Ghastly.Io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ghastly.Controller.Uwp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScenePage : Page
    {
        private SceneSelectionViewModel ViewModel { get; set; }
        public ScenePage()
        {
            this.InitializeComponent();
        }

        public SceneDescription Scene { get; private set; }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var pars = e.Parameter as Tuple<SceneSelectionViewModel, SceneDescription>;
            this.ViewModel = pars.Item1;
            this.Scene = pars.Item2;
            this.SceneTitle.Text = Scene.Name;
            this.PreviewImage.Source = await this.ViewModel.GetSceneImage.Execute(this.Scene.Id).ObserveOnDispatcher();
        }
    }
}
