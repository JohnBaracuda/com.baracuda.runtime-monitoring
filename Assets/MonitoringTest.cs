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
    private int GetUpdateCount(out string str, out  Vector3 dir, out bool[] boolArray, out HideFlags flags)
    {
        flags = HideFlags.NotEditable;
        str = "Hello";
        dir = Vector3.back;
        boolArray = new[] {true, true, false};
        return _updateCounter;
    }
    
    
    private void Update()
    {
        _updateCounter++;
    }
}
