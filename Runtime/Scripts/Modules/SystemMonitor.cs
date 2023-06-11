// Copyright (c) 2022 Jonathan Lang

using System.Globalization;
using UnityEngine;

namespace Baracuda.Monitoring.Modules
{
        /// <summary>
        ///     Custom class showcasing how the monitoring system can be used to create a simple system monitor display.
        /// </summary>
        [MVisible(false)]
    [MOptions(UIPosition.LowerRight)]
    [MFontName("JetBrainsMono-Regular")]
    [MGroupColor(ColorPreset.TransparentBlack)]
    [MGroupName("System")]
    public class SystemMonitor : MonitorModuleBase
    {
        #region Fields

#if MONITORING_EXAMPLES
        [MTag("OS")]
#endif
        [MOrder(6)]
        [Monitor] private string _operatingSystem;
#if MONITORING_EXAMPLES
        [MTag("OS")]
#endif
        [MOrder(6)]
        [Monitor] private string _operatingSystemFamily;

#if MONITORING_EXAMPLES
        [MTag("Device")]
#endif
        [MOrder(5)]
        [Monitor] private string _deviceType;
#if MONITORING_EXAMPLES
        [MTag("Device")]
#endif
        [MOrder(5)]
        [Monitor] private string _deviceModel;
#if MONITORING_EXAMPLES
        [MTag("Device")]
#endif
        [MOrder(5)]
        [Monitor] private string _deviceName;

#if MONITORING_EXAMPLES
        [MTag("CPU")]
#endif
        [MOrder(4)]
        [Monitor] private string _processorType;
#if MONITORING_EXAMPLES
        [MTag("CPU")]
#endif
        [MOrder(4)]
        [Monitor] private string _processorFrequency;
#if MONITORING_EXAMPLES
        [MTag("CPU")]
#endif
        [MOrder(4)]
        [Monitor] private string _processorCount;

#if MONITORING_EXAMPLES
        [MTag("RAM")]
#endif
        [MOrder(3)]
        [Monitor] private string _systemMemory;

#if MONITORING_EXAMPLES
        [MTag("GPU")]
#endif
        [MOrder(2)]
        [Monitor] private string _graphicsDeviceName;
#if MONITORING_EXAMPLES
        [MTag("GPU")]
#endif
        [MOrder(2)]
        [Monitor] private string _graphicsDeviceType;
#if MONITORING_EXAMPLES
        [MTag("GPU")]
#endif
        [MOrder(2)]
        [Monitor] private string _graphicsMemorySize;
#if MONITORING_EXAMPLES
        [MTag("GPU")]
#endif
        [MOrder(2)]
        [Monitor] private string _graphicsMultiThreaded;

#if MONITORING_EXAMPLES
        [MTag("Mobile")]
#endif
        [MOrder(1)]
        [Monitor] private string _batteryLevel;
#if MONITORING_EXAMPLES
        [MTag("Mobile")]
#endif
        [MOrder(1)]
        [Monitor] private string _batteryStatus;

#if MONITORING_EXAMPLES
        [MTag("Path")]
#endif
        [MOrder(0)]
        [Monitor] private string _dataPath;
#if MONITORING_EXAMPLES
        [MTag("Path")]
#endif
        [MOrder(0)]
        [Monitor] private string _persistentDataPath;
#if MONITORING_EXAMPLES
        [MTag("Path")]
#endif
        [MOrder(0)]
        [Monitor] private string _consoleLogPath;
#if MONITORING_EXAMPLES
        [MTag("Path")]
#endif
        [MOrder(0)]
        [Monitor] private string _streamingAssetsPath;
#if MONITORING_EXAMPLES
        [MTag("Path")]
#endif
        [MOrder(0)]
        [Monitor] private string _temporaryCachePath;

        #endregion


        #region Setup

        private void Start()
        {
            _operatingSystem = SystemInfo.operatingSystem;
            _operatingSystemFamily = SystemInfo.operatingSystemFamily.ToString();

            _deviceModel = SystemInfo.deviceModel;
            _deviceName = SystemInfo.deviceName;
            _deviceType = SystemInfo.deviceType.ToString();

            _processorType = SystemInfo.processorType;
            _processorCount = SystemInfo.processorCount.ToString();
            _processorFrequency =
                (SystemInfo.processorFrequency * .001f).ToString("0.00", CultureInfo.InvariantCulture) + "GHz";

            _systemMemory = SystemInfo.systemMemorySize.ToString("N0", CultureInfo.InvariantCulture) + " GB";
            _graphicsDeviceName = SystemInfo.graphicsDeviceName;
            _graphicsDeviceType = SystemInfo.graphicsDeviceType.ToString();
            _graphicsMemorySize = SystemInfo.graphicsMemorySize.ToString("N0", CultureInfo.InvariantCulture) + " GB";
            _graphicsMultiThreaded = SystemInfo.graphicsMultiThreaded.ToString();

            _batteryLevel = SystemInfo.batteryLevel.ToString(CultureInfo.InvariantCulture);
            _batteryStatus = SystemInfo.batteryStatus.ToString();

            _dataPath = Application.dataPath;
            _persistentDataPath = Application.persistentDataPath;
            _consoleLogPath = Application.consoleLogPath;
            _streamingAssetsPath = Application.streamingAssetsPath;
            _temporaryCachePath = Application.temporaryCachePath;
        }

        #endregion
    }
}