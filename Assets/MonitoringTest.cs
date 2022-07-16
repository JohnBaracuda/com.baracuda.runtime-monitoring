using Baracuda.Monitoring;
using UnityEngine;

public class MonitoringTest : MonitoredBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 165;
    }
    
    private static MonitoringTest _instance;

    [Monitor] private bool[] _array1;
    [Monitor] private bool[] _array2;
    [Monitor] private int[] _array3;
    [Monitor] private bool[] _array4 = new []{true, false, true, true};

    [MonitorMethod]
    public bool TryGetInstance(out MonitoringTest instance)
    {
        instance = _instance;
        return _instance != null;
    }

    public readonly ref struct Data
    {
        private readonly long _data;

        public Data(long data)
        {
            _data = data;
        }

        public override string ToString()
        {
            return _data.ToString();
        }
    }
    
    [MonitorMethod]
    public void OutRefStruct(out Data data)
    {
        data = new Data(456456546);
    }
    
    private int _updateCounter;
    
    [MonitorMethod]
    private int GetUpdateCount(out string str, out  Vector3 dir, out bool[] boolArray)
    {
        str = "Hello";
        dir = Vector3.back;
        boolArray = new[] {true, true, false};
        return _updateCounter;
    }
    
    
    private void Update()
    {
        _updateCounter++;
    }

    protected override void Awake()
    {
        base.Awake();
        _instance = this;
    }
}
