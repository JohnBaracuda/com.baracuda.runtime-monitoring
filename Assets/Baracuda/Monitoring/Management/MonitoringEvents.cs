using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Threading;

namespace Baracuda.Monitoring.Management
{
    public static class MonitoringEvents
    {
        #region --- [PROPERTIES] ---

        public static bool IsInitialized
        {
            get => _sIsInitialized;

            [MethodImpl(MethodImplOptions.Synchronized)]
            private set
            {
                if (!Dispatcher.IsMainThread())
                    throw new InvalidOperationException(
                        $"Set => {nameof(IsInitialized)} is only allowed to be set from the main thread!");
                _sIsInitialized = value;
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [BACKING FIELDS] ---

        private static volatile bool _sIsInitialized = false;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [EVENTS] ---

        public delegate void ProfilingCompletedListener(MonitorUnit[] staticUnits, MonitorUnit[] instanceUnits);

        /// <summary>
        /// As the name suggests, this event is called when profiling of the current system has been completed.
        /// This might even be before an Awake call which is why subscribing to this event will instantly invoke
        /// a callback when subscribing after profiling was already completed.
        /// </summary>
        public static event ProfilingCompletedListener ProfilingCompleted
        {
            add
            {
                if (IsInitialized)
                {
                    value.Invoke(MonitoringManager.GetStaticUnits.ToArray(), MonitoringManager.GetInstanceUnits.ToArray());
                    return;
                }
                _profilingCompleted += value;
            }
            remove => _profilingCompleted -= value;
        }

        private static ProfilingCompletedListener _profilingCompleted;
            
            
        public static event Action<MonitorUnit> UnitCreated;
        public static event Action<MonitorUnit> UnitDisposed;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [RAISE] ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RaiseUnitCreated(MonitorUnit monitorUnit)
        {
            if (!Dispatcher.IsMainThread())
            {
                UnitCreated.Dispatch(monitorUnit);
                return;
            }
            UnitCreated?.Invoke(monitorUnit);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RaiseUnitDisposed(MonitorUnit monitorUnit)
        {
            if (!Dispatcher.IsMainThread())
            {
                UnitDisposed.Dispatch(monitorUnit);
                return;
            }
            UnitDisposed?.Invoke(monitorUnit);
        }

        
        internal static void ProfilingCompletedInternal(MonitorUnit[] staticUnits, MonitorUnit[] instanceUnits)
        {
            IsInitialized = true;
            _profilingCompleted?.Invoke(staticUnits, instanceUnits);
            _profilingCompleted = null;
        }
        
        #endregion
    }
}