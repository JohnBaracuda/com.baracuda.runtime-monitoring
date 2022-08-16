// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;
using Baracuda.Utilities.Pooling.Source;

namespace Baracuda.Utilities.Pooling
{
    public class DictionaryPool<TKey, TValue> : CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>
    {
    }
}