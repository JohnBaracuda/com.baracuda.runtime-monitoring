// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System.Collections.Generic;
using Baracuda.Pooling.Abstractions;

namespace Baracuda.Pooling.Concretions
{
    public class ConcurrentListPool<T> : ConcurrentCollectionPool<List<T>, T>
    {
    }
}