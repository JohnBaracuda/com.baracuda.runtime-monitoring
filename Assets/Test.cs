using Baracuda.Monitoring;
using Baracuda.Monitoring.IL2CPP;
using System.Collections.Generic;

public class Test : MonitoredBehaviour
{
    [TypeDef(typeof(MonitoredStruct<Data>))]
    private readonly struct Data
    {
        private readonly int inner;

        public Data(int inner)
        {
            this.inner = inner;
        }

        public override string ToString()
        {
            return inner.ToString();
        }
    }

    [Monitor]
    private IList<int> myList = new List<int>() {1, 2, 3,};

    [Monitor]
    private Dictionary<string, Data> myDataDictionary = new Dictionary<string, Data>()
    {
        {"hello", new Data(3)},
        {"world", new Data(1337)}
    };

    [Monitor]
    private IEnumerable<Data> GetData => new Data[] {new Data(3), new Data(1432)};

    [Monitor]
    private Data[] myDataArray = new Data[] {new Data(123), new Data(13245432), new Data(35)};
}
