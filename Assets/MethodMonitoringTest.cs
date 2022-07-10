using System.Collections;
using System.Collections.Generic;
using Baracuda.Monitoring;
using UnityEngine;

public class MethodMonitoringTest : MonitoredBehaviour
{
    [Monitor] 
    private IList<int> GetInts => new List<int>() {1, 2, 3, 4};

    [Monitor] 
    private IEnumerable<bool> GetBooleans() => new List<bool>() {true, false, true};

    [Monitor]
    [SerializeField] private Transform playerTransform;
    
    private int _updateCounter;

    [Monitor]
    [MValueProcessor(nameof(BooleanProcessor))]
    private bool GetBoolean() => true;

    private string BooleanProcessor(bool value)
    {
        return value ? "Success" : "Failure";
    }
    
    [Monitor]
    private string[] GetStringArray() => new[] {"Aki", "Power", "Denji"};
    
    [MonitorMethod]
    private void GetUpdateCount(out int count)
    {
        count = _updateCounter;
    }
    
    [MonitorMethod(20, true, "Hello World")]
    private string GetDefaultValues(int intValue, bool boolValue, string stringValue)
    {
        return $"\n" +
               $"Int: {intValue.ToString()}\n" +
               $"Bool: {boolValue.ToString()}\n" +
               $"String: {stringValue}";
    }
    
    private void Update()
    {
        _updateCounter++;
    }
}
