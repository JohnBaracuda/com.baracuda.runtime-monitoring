using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Threading;

namespace Baracuda.Monitoring.Management
{
    public static class MonitoringEvents
    {
        #region --- Properties ---

        public static bool IsInitialized
        {
            get => sIsInitialized;

            [MethodImpl(MethodImplOptions.Synchronized)]
            private set
            {
                if (!Dispatcher.IsMainThread())
                {
                    throw new InvalidOperationException(
                        $"Set => {nameof(IsInitialized)} is only allowed to be set from the main thread!");
                }

                sIsInitialized = value;
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Backing Fields ---

        private static volatile bool sIsInitialized = false;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Events ---

        public delegate void ProfilingCompletedListener(IReadOnlyList<IMonitorUnit> staticUnits, IReadOnlyList<IMonitorUnit> instanceUnits);

        /// <summary>
        /// Event is invoked when profiling process for the current system has been completed.
        /// This might even be before an Awake call which is why subscribing to this event will instantly invoke
        /// a callback when subscribing after profiling was already completed.
        /// </summary>
        public static event ProfilingCompletedListener ProfilingCompleted
        {
            add
            {
                if (IsInitialized)
                {
                    value.Invoke(MonitoringManager.GetStaticUnits, MonitoringManager.GetInstanceUnits);
                    return;
                }
                profilingCompleted += value;
            }
            remove => profilingCompleted -= value;
        }

        private static ProfilingCompletedListener profilingCompleted;
            
        /// <summary>
        /// Event is called when a new <see cref="MonitorUnit"/> was created.
        /// </summary>
        public static event Action<IMonitorUnit> UnitCreated;
        
        /// <summary>
        /// Event is called when a <see cref="MonitorUnit"/> was disposed.
        /// </summary>
        public static event Action<IMonitorUnit> UnitDisposed;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Raise ---

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
            profilingCompleted?.Invoke(staticUnits, instanceUnits);
            profilingCompleted = null;
        }
        
        #endregion
    }
}