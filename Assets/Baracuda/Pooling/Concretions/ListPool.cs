using System.Collections.Generic;
using Baracuda.Pooling.Abstractions;

namespace Baracuda.Pooling.Concretions
{
#if JETBRAINS_ANNOTATIONS
    [JetBrains.Annotations.UsedImplicitly]
#endif
    public class ListPool<T> : CollectionPool<List<T>, T>
    {
        
    }
}