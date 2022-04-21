using System.Collections.Generic;
using Baracuda.Monitoring.Utilities.Pooling.Abstractions;

namespace Baracuda.Monitoring.Utilities.Pooling.Concretions
{
    public class DictionaryPool<TKey, TValue> : CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>
    {
    }
}