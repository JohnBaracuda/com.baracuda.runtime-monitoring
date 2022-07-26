// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Baracuda.Pooling.Concretions;
using UnityEngine;

namespace Baracuda.Monitoring.Modules
{
    public class SystemMonitor : MonitorModuleBase
    {
        #region --- Inspector ---

        [Header("Operating System")] 
        [SerializeField] private bool showOperatingSystem = true;
        [SerializeField] private bool showOperatingSystemFamily = false;

        [Header("Device")] 
        [SerializeField] private bool showDeviceType = true;
        [SerializeField] private bool showDeviceModel = false;
        [SerializeField] private bool showDeviceName = false;

        [Header("Processor")] 
        [SerializeField] private bool showProcessorType = true;
        [SerializeField] private bool showProcessorFrequency = false;
        [SerializeField] private bool showProcessorCount = false;

        [Header("Memory")]
        [SerializeField] private bool showSystemMemory = true;
        
        [Header("GPU")]
        [SerializeField] private bool showGraphicsDeviceName = true;
        [SerializeField] private bool showGraphicsDeviceType = true;
        [SerializeField] private bool showGraphicsMemorySize = true;
        [SerializeField] private bool showGraphicsMultiThreaded = false;
        
        [Header("Battery")] 
        [SerializeField] private bool showBatteryLevel = false;
        [SerializeField] private bool showBatteryStatus = false;
        
        #endregion

        #region --- Fields & Events ---

        [Monitor]
        [MUpdateEvent(nameof(DisplayConfigUpdated))]
        [MFormatOptions(UIPosition.LowerRight, GroupElement = false)]
        [MFont("JetBrainsMono-Regular")]
        [MBackgroundColor(ColorPreset.TransparentBlack)]
        [MGroupColor(ColorPreset.Transparent)]
        private string _systemInfo;

        private event Action DisplayConfigUpdated;

        #endregion

        #region --- Setup ---

         private void OnValidate()
        {
            UpdateDisplay();
        }

        private void Start()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            var sb = StringBuilderPool.Get();

            var entries = new List<(string title, string value)>(30);

            if (showOperatingSystem)
            {
                entries.Add(("Operating System", SystemInfo.operatingSystem));
            }
            if (showOperatingSystemFamily)
            {
                entries.Add(("Operating System Family", SystemInfo.operatingSystemFamily.ToString()));
            }

            if (showDeviceModel)
            {
                entries.Add(("Device Model", SystemInfo.deviceModel));
            }
            if (showDeviceName)
            {
                entries.Add(("Device Name", SystemInfo.deviceName));
            }
            if (showDeviceType)
            {
                entries.Add(("Device Type", SystemInfo.deviceType.ToString()));
            }
            
            if (showProcessorType)
            {
                entries.Add(("Processor Type", SystemInfo.processorType));
            }
            if (showProcessorCount)
            {
                entries.Add(("Processor Count", SystemInfo.processorCount.ToString()));
            }
            if (showProcessorFrequency)
            {
                entries.Add(("Processor Frequency", SystemInfo.processorFrequency.ToString()));
            }
            
            if (showSystemMemory)
            {
                entries.Add(("System Memory Size", SystemInfo.systemMemorySize.ToString("N0", CultureInfo.InvariantCulture) + " GB"));
            }
            
            if(showGraphicsDeviceName)
            {
                entries.Add(("Graphics Device Name", SystemInfo.graphicsDeviceName));
            }
            if(showGraphicsDeviceType)
            {
                entries.Add(("Graphics Device Type", SystemInfo.graphicsDeviceType.ToString()));
            }
            if(showGraphicsMemorySize)
            {
                entries.Add(("Graphics Memory Size", SystemInfo.graphicsMemorySize.ToString("N0", CultureInfo.InvariantCulture) + " GB"));
            }
            if(showGraphicsMultiThreaded)
            {
                entries.Add(("Graphics Multi Threaded", SystemInfo.graphicsMultiThreaded.ToString()));
            }
            
            if (showBatteryLevel)
            {
                entries.Add(("Battery Level", SystemInfo.batteryLevel.ToString(CultureInfo.InvariantCulture)));
            }
            if (showBatteryStatus)
            {
                entries.Add(("Battery Status", SystemInfo.batteryStatus.ToString()));
            }

            var max = entries.Select(valueTuple => valueTuple.title.Length).Prepend(0).Max();

            foreach (var (title, value) in entries)
            {
                sb.Append('\n');
                sb.Append(title);
                sb.Append(':');
                sb.Append(new string(' ', max + 1 - title.Length));
                sb.Append(value);
            }

            _systemInfo = StringBuilderPool.Release(sb);

            DisplayConfigUpdated?.Invoke();
        }
        
        #endregion
    }
}