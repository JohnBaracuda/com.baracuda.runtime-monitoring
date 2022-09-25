// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;

namespace Baracuda.Monitoring.Utilities.Pooling
{
    internal class ListPool<T> : ConcurrentCollectionPool<List<T>, T>
    {
    }
}