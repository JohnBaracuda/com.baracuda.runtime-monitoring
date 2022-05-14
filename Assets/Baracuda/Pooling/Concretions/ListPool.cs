using System.Collections.Generic;
using Baracuda.Pooling.Abstractions;

namespace Baracuda.Pooling.Concretions
{
    public class ListPool<T> : CollectionPool<List<T>, T>
    {
    }
}