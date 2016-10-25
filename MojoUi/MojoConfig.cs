using System;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;

namespace MojoUi
{
    internal class MojoConfig
    {
        public static int BigCacheLimit => 256;
        public static IScheduler MainThreadScheduler { get; }

        static MojoConfig()
        {
            MainThreadScheduler = DefaultScheduler.Instance;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        internal static void EnsureInitialized()
        {
            // NB: This method only exists to invoke the static constructor
        }
    }
}