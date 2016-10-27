using Ghastly.Io;
using MojoUi;
using System;
using System.Collections.Generic;
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

        public ReactiveCommand<Unit, IEnumerable<SceneDescription>> LoadScenes { get; }
        public ReactiveCommand Trigger { get; }

        public SceneSelectionViewModel(IGhastlyService ghast = null)
        {
            ghast = ghast ?? new MockGhast();
            this.LoadScenes = ReactiveCommand.CreateFromTask(ghast.GetScenes);
            this._Scenes = this.LoadScenes.ObserveOnDispatcher().StartWith(MockScenes).ToProperty(this, x => x.Scenes);
            this.Trigger = ReactiveCommand.CreateFromTask(ghast.ActivateScene);
        }

        private class MockGhast : IGhastlyService
        {
            public Task ActivateScene() => Task.CompletedTask;

            public Task<IEnumerable<SceneDescription>> GetScenes() => Task.FromResult(Enumerable.Empty<SceneDescription>());
        }
    }
}
