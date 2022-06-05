// Copyright (c) 2022 Jonathan Lang

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
        
        [Monitor]
        [MValueProcessor(nameof(FPSProcessor))]
        [MUpdateEvent(nameof(FPSUpdated))]
        [MFormatOptions(FontSize = 32, Position = UIPosition.UpperRight, GroupElement = false)]
        private float _fps;

        /*
         *  Events   
         */

        public static event Action<float> FPSUpdated;
        
        //--------------------------------------------------------------------------------------------------------------
        
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

        #region --- Vsync ---

        [Monitor] 
        [MFormatOptions(FontSize = 16, Position = UIPosition.UpperRight, GroupElement = false)]
        [MValueProcessor(nameof(ProcessorTargetFrameRate))]
        private int TargetFrameRate => Application.targetFrameRate;

        private static string ProcessorTargetFrameRate(int value)
        {
            return $"Target Framerate: {(value > 0 ? value.ToString() : "Unlimited")}";
        }
        
        [Monitor] 
        [MFormatOptions(FontSize = 16, Position = UIPosition.UpperRight, GroupElement = false)]
        [MValueProcessor(nameof(ProcessorVsync))]
        private int Vsync => QualitySettings.vSyncCount;

        private static string ProcessorVsync(int value)
        {
            return $"Vsync: {(value > 0 ? $"Vsync Count: {value}" : "Disabled")}";
        }
        
        #endregion        
        
    }
}