using System;

namespace Baracuda.Monitoring.Experimental
{
    public interface IMonitoredType
    {
        bool IsDirty { get; }
        void SetDirty();
        event Action BecameDirty;
    }
}
