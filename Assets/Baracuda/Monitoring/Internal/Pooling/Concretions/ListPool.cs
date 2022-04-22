using System.Collections.Generic;
using Baracuda.Monitoring.Internal.Pooling.Abstractions;

namespace Baracuda.Monitoring.Internal.Pooling.Concretions
{
    public class ListPool<T> : CollectionPool<List<T>, T>
    {
    }
}