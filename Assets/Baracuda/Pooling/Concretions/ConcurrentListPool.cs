using System.Collections.Generic;
using Baracuda.Pooling.Abstractions;

namespace Baracuda.Pooling.Concretions
{
#if JETBRAINS_ANNOTATIONS
    [JetBrains.Annotations.UsedImplicitly]
#endif
    public class ConcurrentListPool<T> : ConcurrentCollectionPool<List<T>, T>
    {
        
    }
}