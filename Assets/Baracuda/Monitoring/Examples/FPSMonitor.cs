using System;
using System.Text;
using Baracuda.Attributes.Monitoring;
using Baracuda.Monitoring.Internal;
using UnityEngine;

namespace Baracuda.Monitoring.Examples
{
    public static class FPSMonitor
    {
        #region --- [FIELDS] ---

        public const float MEASURE_PERIOD = 0.25f;
        private const string COLOR_MIN_MARKUP = "<color=#07fc03>";
        private const string COLOR_MID_MARKUP = "<color=#fcba03>";
        private const string C_MAX = "<color=#07fc03>";
        private const int THRESHOLD_ONE = 30;
        private const int THRESHOLD_TWO = 60;
        
        private static int _frameCount = 0;
        private static float _timer = 0;
        private static int _lastFPS;
        private static float _lastMeasuredFps = 0;

        private static readonly StringBuilder _stringBuilder = new StringBuilder();

        #endregion
        
        #region --- [EVENTS] ---

        public static event Action<float> FPSUpdated;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        
        [MonitorValue(Update = nameof(FPSUpdated))]
        [ValueProcessor(nameof(Processor))]
        [MonitorDisplayOptions(FontSize = 32, Position = UIPosition.TopRight, GroupElement = false)]
        private static float _fps;

        
        
        public static string Processor(float value)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append('[');
            _stringBuilder.Append(value >= THRESHOLD_TWO ? C_MAX : value >= THRESHOLD_ONE ? COLOR_MID_MARKUP : COLOR_MIN_MARKUP);
            _stringBuilder.Append(value.ToString("00.00"));
            _stringBuilder.Append("</color>]");
            return _stringBuilder.ToString();
        }
        
        private static void Update()
        {
            _frameCount++;
            _timer += Time.deltaTime / Time.timeScale;

            if (_timer < MEASURE_PERIOD) return;

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
        
        //--------------------------------------------------------------------------------------------------------------
        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            MonitoringUpdateHook.EnsureExist().OnUpdate += Update;
        }
    }
}