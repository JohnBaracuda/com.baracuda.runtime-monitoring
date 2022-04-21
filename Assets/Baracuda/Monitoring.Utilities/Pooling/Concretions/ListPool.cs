using System.Collections.Generic;
using Baracuda.Monitoring.Utilities.Pooling.Abstractions;

namespace Baracuda.Monitoring.Utilities.Pooling.Concretions
{
    public class ListPool<T> : CollectionPool<List<T>, T>
    {
    }
}