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
    }
}