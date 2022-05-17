// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using Baracuda.Monitoring.API;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class MonitorInput : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                MonitoringUI.ToggleDisplay();
            }
        }
    }
}
