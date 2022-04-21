using System.Collections.Generic;
using Baracuda.Monitoring.Utilities.Pooling.Abstractions;

namespace Baracuda.Monitoring.Utilities.Pooling.Concretions
{
    public class HashSetPool<T> : CollectionPool<HashSet<T>, T>
    {
    }
}