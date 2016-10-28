using Ghastly.Controller.Uwp.ViewModels;
using System.Reactive.Linq;
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
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            App.Active.GhastlyHost
                .Subscribe(host =>
                {
                    this.HostTextBox.ClearValue(TextBox.TextProperty);
                    this.HostTextBox.PlaceholderText = host;
                });
        }



        public SceneSelectionViewModel ViewModel
        {
            get { return (SceneSelectionViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(SceneSelectionViewModel), typeof(SettingsPage), new PropertyMetadata(null));



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.ViewModel = e.Parameter as SceneSelectionViewModel;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.LoadScenes.Execute();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            App.Active.GhastlyHost.OnNext(this.HostTextBox.Text);
        }
    }
}
