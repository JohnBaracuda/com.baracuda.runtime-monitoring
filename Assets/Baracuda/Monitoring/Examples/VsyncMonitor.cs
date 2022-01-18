using Baracuda.Attributes;
using Baracuda.Attributes.Monitoring;
using UnityEngine;

namespace Baracuda.Monitoring.Examples
{
    public static class VsyncMonitor
    {
        [MonitorProperty]
        [MonitorDisplayOptions(FontSize = 16, Position = UIPosition.TopRight, GroupElement = false)]
        [ValueProcessor(nameof(ProcessorTargetFrameRate))]
        private static int TargetFrameRate => Application.targetFrameRate;

        private static string ProcessorTargetFrameRate(int value)
        {
            return $"Target Framerate: {(value > 0 ? value.ToString() : "Unlimited")}";
        }
        
        [MonitorProperty]
        [MonitorDisplayOptions(FontSize = 16, Position = UIPosition.TopRight, GroupElement = false)]
        [ValueProcessor(nameof(ProcessorVsync))]
        private static int Vsync => QualitySettings.vSyncCount;

        private static string ProcessorVsync(int value)
        {
            return $"Vsync: {(value > 0 ? "Disabled" : "Enabled")}";
        }
    }
}