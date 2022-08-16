// Copyright (c) 2022 Jonathan Lang
 
using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring.Source.Systems
{
    internal class MonitoringPluginData : IMonitoringPlugin
    {
        public string Copyright { get; } = "© 2022 Jonathan Lang";
        public string Version { get; } = "2.1.2";
        public string Documentation { get; } = "https://johnbaracuda.com/monitoring.html";
        public string Repository { get; } = "https://github.com/johnbaracuda/Runtime-Monitoring";
        public string Website { get; } = "https://johnbaracuda.com/";
    }
}