using System.Collections;
using System.Collections.Generic;
using Baracuda.Monitoring;
using UnityEngine;

public class MethodMonitoringTest : MonitoredBehaviour
{
    private static MethodMonitoringTest _instance;

    [MonitorMethod]
    public bool TryGetInstance(out MethodMonitoringTest instance)
    {
        instance = _instance;
        return _instance != null;
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
