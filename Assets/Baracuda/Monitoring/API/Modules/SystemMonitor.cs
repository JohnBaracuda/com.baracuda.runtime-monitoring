// Copyright (c) 2022 Jonathan Lang

using System.Globalization;
using UnityEngine;

namespace Baracuda.Monitoring.Modules
{
    [MVisible(false)]
    [MOptions(UIPosition.LowerRight)]
    [MFontName("JetBrainsMono-Regular")]
    [MGroupColor(ColorPreset.TransparentBlack)]
    [MGroupName("System")]
    public class SystemMonitor : MonitorModuleBase
    {
        #region --- Fields ---

        [MTag("OS")][MOrder(6)]
        [Monitor] private string _operatingSystem;
        [MTag("OS")][MOrder(6)]
        [Monitor] private string _operatingSystemFamily;

        [MTag("Device")][MOrder(5)]
        [Monitor] private string _deviceType;
        [MTag("Device")][MOrder(5)]
        [Monitor] private string _deviceModel;
        [MTag("Device")][MOrder(5)]
        [Monitor] private string _deviceName;

        [MTag("CPU")][MOrder(4)]
        [Monitor] private string _processorType;
        [MTag("CPU")][MOrder(4)]
        [Monitor] private string _processorFrequency;
        [MTag("CPU")][MOrder(4)]
        [Monitor] private string _processorCount;

        [MTag("RAM")][MOrder(3)]
        [Monitor] private string _systemMemory;

        [MTag("GPU")][MOrder(2)]
        [Monitor] private string _graphicsDeviceName;
        [MTag("GPU")][MOrder(2)]
        [Monitor] private string _graphicsDeviceType;
        [MTag("GPU")][MOrder(2)]
        [Monitor] private string _graphicsMemorySize;
        [MTag("GPU")][MOrder(2)]
        [Monitor] private string _graphicsMultiThreaded;

        [MTag("Mobile")][MOrder(1)]
        [Monitor] private string _batteryLevel;
        [MTag("Mobile")][MOrder(1)]
        [Monitor] private string _batteryStatus;
        
        [MTag("Path")][MOrder(0)]
        [Monitor] private string _dataPath;
        [MTag("Path")][MOrder(0)]
        [Monitor] private string _persistentDataPath;
        [MTag("Path")][MOrder(0)]
        [Monitor] private string _consoleLogPath;
        [MTag("Path")][MOrder(0)]
        [Monitor] private string _streamingAssetsPath;
        [MTag("Path")][MOrder(0)]
        [Monitor] private string _temporaryCachePath;
        
        #endregion

        #region --- Setup ---

        private void Start()
        {
            _operatingSystem = SystemInfo.operatingSystem;
            _operatingSystemFamily = SystemInfo.operatingSystemFamily.ToString();

            _deviceModel = SystemInfo.deviceModel;
            _deviceName = SystemInfo.deviceName;
            _deviceType = SystemInfo.deviceType.ToString();
            
            _processorType = SystemInfo.processorType;
            _processorCount = SystemInfo.processorCount.ToString();
            _processorFrequency = (SystemInfo.processorFrequency * .001f).ToString("0.00", CultureInfo.InvariantCulture) + "GHz";

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