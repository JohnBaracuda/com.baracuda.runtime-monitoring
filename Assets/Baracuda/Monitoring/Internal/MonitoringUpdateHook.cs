using System;
using System.Threading.Tasks;
using Baracuda.Monitoring.Internal.Utils;
using Baracuda.Threading;

namespace Baracuda.Monitoring.Internal
{
    internal class MonitoringUpdateHook : MonoSingleton<MonitoringUpdateHook>
    {
        internal event Action OnUpdate;
        internal event Action OnTick;

        private void Update()  => OnUpdate?.Invoke();
        private void Tick() => OnTick?.Invoke();
        
        private async void TickLoop()
        {
            try
            {
                while (true)
                {
                    Tick();
                    await Task.Delay(50, Dispatcher.RuntimeToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        protected override void Awake()
        {
            base.Awake();
            TickLoop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnUpdate = null;
            OnTick = null;
        }
    }
}