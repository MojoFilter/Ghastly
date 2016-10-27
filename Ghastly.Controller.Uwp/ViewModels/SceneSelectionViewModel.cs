﻿using Ghastly.Io;
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
        public ReactiveCommand Trigger { get; }

        public SceneSelectionViewModel(IGhastlyService ghast)
        {
            this.LoadScenes = ReactiveCommand.CreateFromTask(ghast.GetScenes);
            this._Scenes = this.LoadScenes.ObserveOnDispatcher().ToProperty(this, x => x.Scenes);
            this.Trigger = ReactiveCommand.CreateFromTask(ghast.ActivateScene);
        }
    }
}
