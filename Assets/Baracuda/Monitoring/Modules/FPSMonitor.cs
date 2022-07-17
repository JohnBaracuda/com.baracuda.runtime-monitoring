// Copyright (c) 2022 Jonathan Lang

using System;
using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Modules
{
    public class FPSMonitor : MonitorModuleBase
    {
#pragma warning disable
        /*
         *  Fields   
         */

        private const float MEASURE_PERIOD = 0.25f;
        private const string COLOR_MIN_MARKUP = "<color=#07fc03>";
        private const string COLOR_MID_MARKUP = "<color=#fcba03>";
        private const string C_MAX = "<color=#07fc03>";
        private const int THRESHOLD_ONE = 30;
        private const int THRESHOLD_TWO = 60;
        
        private float _timer = 0;
        private int _lastFPS;
        private float _lastMeasuredFps = 0;
        private int _frameCount = 0;

        private readonly StringBuilder _stringBuilder = new StringBuilder();

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

        public event Action<float> FPSUpdated;
        
        //--------------------------------------------------------------------------------------------------------------
        
        public string FPSProcessor(float value)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append('[');
            _stringBuilder.Append(value >= THRESHOLD_TWO ? C_MAX : value >= THRESHOLD_ONE ? COLOR_MID_MARKUP : COLOR_MIN_MARKUP);
            _stringBuilder.Append(value.ToString("00.00"));
            _stringBuilder.Append("</color>]");
            return _stringBuilder.ToString();
        }
        
        private void Update()
        {
            _frameCount++;
            _timer += Time.deltaTime / Time.timeScale;

            if (_timer < MEASURE_PERIOD)
            {
                return;
            }

            _lastMeasuredFps = (_frameCount / _timer);

            if (Math.Abs(_lastMeasuredFps - _lastFPS) > .1f)
            {
                _fps = _lastMeasuredFps;
                FPSUpdated?.Invoke(_fps);
            }
                

            _lastFPS = _frameCount;
            _frameCount = 0;

            var rest = MEASURE_PERIOD - _timer;
            _timer = rest;
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