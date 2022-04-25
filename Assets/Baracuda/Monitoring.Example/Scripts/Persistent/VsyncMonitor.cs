using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts.Persistent
{
    public static class VsyncMonitor
    {
        [Monitor] 
        [Format(FontSize = 16, Position = UIPosition.TopRight, GroupElement = false)]
        [ValueProcessor(nameof(ProcessorTargetFrameRate))]
        private static int TargetFrameRate => Application.targetFrameRate;

        private static string ProcessorTargetFrameRate(int value)
        {
            return $"Target Framerate: {(value > 0 ? value.ToString() : "Unlimited")}";
        }
        
        [Monitor] 
        [Format(FontSize = 16, Position = UIPosition.TopRight, GroupElement = false)]
        [ValueProcessor(nameof(ProcessorVsync))]
        private static int Vsync => QualitySettings.vSyncCount;

        private static string ProcessorVsync(int value)
        {
            return $"Vsync: {(value > 0 ? $"Vsync Count: {value}" : "Disabled")}";
        }
    }
}