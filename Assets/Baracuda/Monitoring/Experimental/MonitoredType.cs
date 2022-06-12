using System;

namespace Baracuda.Monitoring.Experimental
{
    public class MonitoredType : IMonitoredType
    {
        private bool _isDirty = true;
        
        public bool IsDirty
        {
            get => _isDirty;
            internal set
            {
                if (value && !_isDirty)
                {
                    _isDirty = true;
                    BecameDirty?.Invoke();
                    return;
                }

                _isDirty = value;
            }
        }

        public void SetDirty()
        {
            IsDirty = true;
        }

        public event Action BecameDirty;
    }
}