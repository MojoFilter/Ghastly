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

        private ObservablePropertySource<IEnumerable<SceneDescription>> _Scenes;
        public IEnumerable<SceneDescription> Scenes { get { return _Scenes.Value; } }

        public ReactiveCommand<Unit, IEnumerable<SceneDescription>> LoadScenes { get; }

        public SceneSelectionViewModel(IGhastlyService ghast)
        {
            this.LoadScenes = ReactiveCommand.CreateFromTask(async _ => (await ghast.GetScenes().ToList()).AsEnumerable());
            this._Scenes = this.LoadScenes.ToProperty(this, x => x.Scenes);
        }
    }
}
