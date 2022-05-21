// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts.Persistent
{
    public class FPSMonitor : MonitoredSingleton<FPSMonitor>
    {
#pragma warning disable
        /*
         *  Fields   
         */
        
        public const float MEASURE_PERIOD = 0.25f;
        private const string COLOR_MIN_MARKUP = "<color=#07fc03>";
        private const string COLOR_MID_MARKUP = "<color=#fcba03>";
        private const string C_MAX = "<color=#07fc03>";
        private const int THRESHOLD_ONE = 30;
        private const int THRESHOLD_TWO = 60;
        
        private static float timer = 0;
        private static int lastFPS;
        private static float lastMeasuredFps = 0;
        private static int frameCount = 0;

        private static readonly StringBuilder stringBuilder = new StringBuilder();

        /*
         *  FPS Monitor   
         */
        
        [MonitorValue(UpdateEvent = nameof(FPSUpdated))]
        [ValueProcessor(nameof(FPSProcessor))]
        [Format(FontSize = 32, Position = UIPosition.UpperRight, GroupElement = false)]
        private float _fps;

#if FPS_MONITOR_TOTAL
        //[MonitorValue(Update = UpdateOptions.TickUpdate)]
#endif
        private long _totalFrameCount = 0;
        
#if FPS_MONITOR_TOTAL_FIXED
        //[MonitorValue(Update = UpdateOptions.TickUpdate)]
#endif
        private long _fixedUpdateCount = 0;

        /*
         *  Events   
         */

        public static event Action<float> FPSUpdated;
        
        //--------------------------------------------------------------------------------------------------------------
        
#if FPS_MONITOR_FORCE
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeOnLoad()
        {
            Promise();
        }
#endif
        
        public static string FPSProcessor(float value)
        {
            stringBuilder.Clear();
            stringBuilder.Append('[');
            stringBuilder.Append(value >= THRESHOLD_TWO ? C_MAX : value >= THRESHOLD_ONE ? COLOR_MID_MARKUP : COLOR_MIN_MARKUP);
            stringBuilder.Append(value.ToString("00.00"));
            stringBuilder.Append("</color>]");
            return stringBuilder.ToString();
        }
        
        private void Update()
        {
            frameCount++;
            _totalFrameCount++;
            timer += Time.deltaTime / Time.timeScale;

            if (timer < MEASURE_PERIOD)
            {
                return;
            }

            lastMeasuredFps = (frameCount / timer);

            if (Math.Abs(lastMeasuredFps - lastFPS) > .1f)
            {
                _fps = lastMeasuredFps;
                FPSUpdated?.Invoke(_fps);
            }
                

            lastFPS = frameCount;
            frameCount = 0;

            var rest = MEASURE_PERIOD - timer;
            timer = rest;
        }

        private void FixedUpdate()
        {
            _fixedUpdateCount++;
        }

        #region --- Vsync ---

        [Monitor] 
        [Format(FontSize = 16, Position = UIPosition.UpperRight, GroupElement = false)]
        [ValueProcessor(nameof(ProcessorTargetFrameRate))]
        private int TargetFrameRate => Application.targetFrameRate;

        private static string ProcessorTargetFrameRate(int value)
        {
            return $"Target Framerate: {(value > 0 ? value.ToString() : "Unlimited")}";
        }
        
        [Monitor] 
        [Format(FontSize = 16, Position = UIPosition.UpperRight, GroupElement = false)]
        [ValueProcessor(nameof(ProcessorVsync))]
        private int Vsync => QualitySettings.vSyncCount;

        private static string ProcessorVsync(int value)
        {
            return $"Vsync: {(value > 0 ? $"Vsync Count: {value}" : "Disabled")}";
        }
        
        #endregion        
        
    }
}