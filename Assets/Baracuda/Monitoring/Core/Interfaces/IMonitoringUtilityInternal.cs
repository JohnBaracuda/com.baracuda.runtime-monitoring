// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring.Interfaces
{
    internal interface IMonitoringUtilityInternal : IMonitoringSubsystem<IMonitoringUtilityInternal>
    {
        void AddFontHash(int fontHash);
        void AddTag(string tag);
        void AddTypeString(string typeString);
    }
}