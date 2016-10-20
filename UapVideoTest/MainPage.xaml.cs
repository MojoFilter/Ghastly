using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        private IRandomAccessStream[] streams;

        private int index = 0;
        private string contentType;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await StartBg();
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await this.StartImmediate();
        }
    }
}
