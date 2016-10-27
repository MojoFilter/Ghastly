using Ghastly.Controller.Uwp.ViewModels;
using Ghastly.Controller.Uwp.Views;
using Ghastly.Io;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Ghastly.Controller.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.ViewModel = new SceneSelectionViewModel();// new TcpGhastlyService("bahamut", 11337));
        }



        public SceneSelectionViewModel ViewModel
        {
            get { return (SceneSelectionViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(SceneSelectionViewModel), typeof(MainPage), new PropertyMetadata(null));



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.ScenarioFrame.Navigate(typeof(SettingsPage));
        }

        private void Hamburger_Click(object sender, RoutedEventArgs e)
        {
            this.Splitter.IsPaneOpen = !this.Splitter.IsPaneOpen;
        }

        private void ScenarioControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var news = e.AddedItems.OfType<SceneDescription>();
            if (news.Any())
            {
                this.ScenarioFrame.Navigate(typeof(ScenePage), Tuple.Create(this.ViewModel, news.First()));
            }
        }

        private void Footer_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            this.ScenarioFrame.Navigate(typeof(SettingsPage));
        }
    }
}
