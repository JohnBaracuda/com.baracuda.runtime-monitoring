using System.Collections.Generic;
using Baracuda.Monitoring.Utilities.Pooling.Abstractions;

namespace Baracuda.Monitoring.Utilities.Pooling.Concretions
{
    public class ConcurrentListPool<T> : ConcurrentCollectionPool<List<T>, T>
    {
    }
}