// Copyright (c) 2022 Jonathan Lang

using System.Collections;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] [Min(30)] private int maxFrameRate = 165;
        [SerializeField] [Range(0,4)] private int vsyncCount = 0;
        [SerializeField] private bool logInit = false;
        
        private void Awake()
        {
            QualitySettings.vSyncCount = vsyncCount;
            Application.targetFrameRate = maxFrameRate;

            if (logInit)
            {
                Debug.Log($"Setting vSync to [{QualitySettings.vSyncCount}]!", this);
                Debug.Log($"Setting target frame rate to [{Application.targetFrameRate}]!", this);
            }
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(3);
            
            Debug.Log("[Tip] Press F3 to toggle the monitoring display.");
            
            yield return new WaitForSeconds(3);
            
            Debug.Log("[Tip] Press F5 to open a filtering input field.");
        }
    }
}
