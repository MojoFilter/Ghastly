using System;
using System.Runtime.CompilerServices;
using Etg.SimpleStubs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghastly.Io
{
    [CompilerGenerated]
    public class StubIGhastlyCommand : IGhastlyCommand
    {
        private readonly StubContainer<StubIGhastlyCommand> _stubs = new StubContainer<StubIGhastlyCommand>();
    }
}

namespace Ghastly.Io
{
    [CompilerGenerated]
    public class StubIGhastlyService : IGhastlyService
    {
        private readonly StubContainer<StubIGhastlyService> _stubs = new StubContainer<StubIGhastlyService>();

        global::System.Threading.Tasks.Task<global::System.Collections.Generic.IEnumerable<global::Ghastly.Io.SceneDescription>> global::Ghastly.Io.IGhastlyService.GetScenes()
        {
            return _stubs.GetMethodStub<GetScenes_Delegate>("GetScenes").Invoke();
        }

        public delegate global::System.Threading.Tasks.Task<global::System.Collections.Generic.IEnumerable<global::Ghastly.Io.SceneDescription>> GetScenes_Delegate();

        public StubIGhastlyService GetScenes(GetScenes_Delegate del, int count = Times.Forever, bool overwrite = false)
        {
            _stubs.SetMethodStub(del, count, overwrite);
            return this;
        }

        global::System.Threading.Tasks.Task global::Ghastly.Io.IGhastlyService.ActivateScene()
        {
            return _stubs.GetMethodStub<ActivateScene_Delegate>("ActivateScene").Invoke();
        }

        public delegate global::System.Threading.Tasks.Task ActivateScene_Delegate();

        public StubIGhastlyService ActivateScene(ActivateScene_Delegate del, int count = Times.Forever, bool overwrite = false)
        {
            _stubs.SetMethodStub(del, count, overwrite);
            return this;
        }

        global::System.Threading.Tasks.Task<int> global::Ghastly.Io.IGhastlyService.GetCurrentSceneId()
        {
            return _stubs.GetMethodStub<GetCurrentSceneId_Delegate>("GetCurrentSceneId").Invoke();
        }

        public delegate global::System.Threading.Tasks.Task<int> GetCurrentSceneId_Delegate();

        public StubIGhastlyService GetCurrentSceneId(GetCurrentSceneId_Delegate del, int count = Times.Forever, bool overwrite = false)
        {
            _stubs.SetMethodStub(del, count, overwrite);
            return this;
        }

        global::System.Threading.Tasks.Task global::Ghastly.Io.IGhastlyService.BeginScene(int sceneId)
        {
            return _stubs.GetMethodStub<BeginScene_Int32_Delegate>("BeginScene").Invoke(sceneId);
        }

        public delegate global::System.Threading.Tasks.Task BeginScene_Int32_Delegate(int sceneId);

        public StubIGhastlyService BeginScene(BeginScene_Int32_Delegate del, int count = Times.Forever, bool overwrite = false)
        {
            _stubs.SetMethodStub(del, count, overwrite);
            return this;
        }
    }
}