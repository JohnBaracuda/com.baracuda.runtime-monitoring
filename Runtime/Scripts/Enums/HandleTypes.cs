using System;

namespace Baracuda.Monitoring
{
#pragma warning disable CS0067
    [Flags]
    public enum HandleTypes
    {
        None = 0,
        Instance = 1,
        Static = 2,
        All = 3
    }
}