using Ghastly.Controller.Uwp.ViewModels;
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
            this.selection.ViewModel = new SceneSelectionViewModel(new TcpGhastlyService("bahamut", 11337));
        }

        //private VM ViewModel { get; set; }

        //private class VM : ReactiveObject
        //{
        //    public VM()
        //    {
        //        var src = Observable.Interval(TimeSpan.FromSeconds(1.0)).Publish().RefCount();

        //        src.Subscribe(t => Debug.WriteLine(t));
        //        src.ObserveOn(RxApp.MainThreadScheduler).Subscribe(t => this.Reg = t);
        //        this._Help = src.ToProperty(this, x => x.Help,0, RxApp.MainThreadScheduler);
        //    }

        //    private long _Reg;
        //    public long Reg
        //    {
        //        get { return this._Reg; }
        //        set { this.RaiseAndSetIfChanged(ref _Reg, value); }
        //    }

        //    private ObservableAsPropertyHelper<long> _Help;
        //    public long Help { get { return _Help.Value; } }
        //}
    }
}
