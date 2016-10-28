using Ghastly.Io;
using MojoUi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghastly.Controller.Uwp.ViewModels
{
    public class SceneSelectionViewModel : MojoObject
    {

        private static readonly IEnumerable<SceneDescription> MockScenes = new[] {
            new SceneDescription() { Name = "Some Skeletons" },
            new SceneDescription() { Name = "Blood and stuff" }
        };

        private ObservablePropertySource<IEnumerable<SceneDescription>> _Scenes;
        public IEnumerable<SceneDescription> Scenes { get { return _Scenes.Value; } }

        //private ObservablePropertySource<SceneDescription> _CurrentScene;
        //public SceneDescription CurrentScene { get { return _CurrentScene.Value; } }

        public ReactiveCommand<Unit, IEnumerable<SceneDescription>> LoadScenes { get; }
        public ReactiveCommand Trigger { get; }
        public ReactiveCommand<int, Unit> BeginScene { get; }

        public SceneSelectionViewModel(IGhastlyService ghast = null)
        {
            ghast = ghast ?? new MockGhast();
            this.LoadScenes = ReactiveCommand.CreateFromTask(ghast.GetScenes);
            this._Scenes = this.LoadScenes
                .Select(scenes=>scenes.OrderBy(s=>s.Name))
                .ObserveOnDispatcher()
                //.StartWith(MockScenes)
                .ToProperty(this, x => x.Scenes);
            this.Trigger = ReactiveCommand.CreateFromTask(ghast.ActivateScene);
            this.BeginScene = ReactiveCommand.CreateFromTask<int>(ghast.BeginScene);

            this.LoadScenes.Subscribe(scenes => Debug.Write(scenes));

            ((INotifyPropertyChanged)this).PropertyChanged += SceneSelectionViewModel_PropertyChanged;
            //this._CurrentScene =
            //    Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(2.0))
            //        .SelectMany(_ => ghast.GetCurrentSceneId())
            //        .Retry()
            //        .Select(id => this.Scenes.FirstOrDefault(s => s.Id == id))
            //        .ToProperty(this, x => x.CurrentScene);
        }

        private void SceneSelectionViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("Changed: " + e.PropertyName);
        }

        private class MockGhast : IGhastlyService
        {
            public Task ActivateScene() => Task.CompletedTask;

            public Task BeginScene(int sceneId) => Task.CompletedTask;

            public Task<int> GetCurrentSceneId() => Task.FromResult(0);

            public Task<IEnumerable<SceneDescription>> GetScenes() => Task.FromResult(Enumerable.Empty<SceneDescription>());
        }
    }
}
