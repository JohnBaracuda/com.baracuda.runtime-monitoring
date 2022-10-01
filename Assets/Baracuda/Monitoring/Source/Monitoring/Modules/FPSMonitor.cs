// Copyright (c) 2022 Jonathan Lang

using System;
using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Modules
{
    /// <summary>
    /// Custom class showcasing how the monitoring system can be used to create a simple FPS display.
    /// </summary>
    public class FPSMonitor : MonitorModuleBase
    {
        #region --- Inspector Fields ---

        [Header("Measurement")] [Range(0.1f, 2f)] [SerializeField]
        private float measurePeriod = 0.25f;

        [Header("Coloring")] [SerializeField] private Color minColor = Color.red;
        [SerializeField] private Color midColor = Color.yellow;
        [SerializeField] private Color maxColor = Color.green;

        #endregion

        #region --- Member Variables ---

        /*
         *  Fields
         */

        private const int THRESHOLD_ONE = 30;
        private const int THRESHOLD_TWO = 60;

        private float _timer = 0;
        private int _lastFPS;
        private float _lastMeasuredFps = 0;
        private int _frameCount = 0;

        private string _colorMinMarkup = "<color=#07fc03>";
        private string _colorMidMarkup = "<color=#fcba03>";
        private string _colorMaxMarkup = "<color=#07fc03>";

        private readonly StringBuilder _fpsBuilder = new StringBuilder();
        private readonly StringBuilder _vsyncBuilder = new StringBuilder();
        private readonly StringBuilder _targetBuilder = new StringBuilder();

        /*
         *  Events
         */

        /// <summary>
        /// Event is invoked evey time the FPS display is updated.
        /// </summary>
        public event Action<float> FPSUpdated;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Setup ---

        /*
         * Create Color Markup
         */

        private void Start()
        {
            GenerateColorMarkup();
        }

        private void OnValidate()
        {
            GenerateColorMarkup();
        }

        private void GenerateColorMarkup()
        {
            _colorMinMarkup = $"<color=#{ColorUtility.ToHtmlStringRGB(minColor)}>";
            _colorMidMarkup = $"<color=#{ColorUtility.ToHtmlStringRGB(midColor)}>";
            _colorMaxMarkup = $"<color=#{ColorUtility.ToHtmlStringRGB(maxColor)}>";
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- FPS ---

        [Monitor]
        [MOrder(1000)]
        [MFontSize(32)]
        [MGroupElement(false)]
        [MPosition(UIPosition.UpperRight)]
        [MUpdateEvent(nameof(FPSUpdated))]
        [MValueProcessor(nameof(FPSProcessor))]
        private float _fps;

        private string FPSProcessor(float value)
        {
            _fpsBuilder.Clear();
            _fpsBuilder.Append('[');
            _fpsBuilder.Append(value >= THRESHOLD_TWO ? _colorMaxMarkup :
                value >= THRESHOLD_ONE ? _colorMidMarkup : _colorMinMarkup);
            _fpsBuilder.Append(value.ToString("00.00"));
            _fpsBuilder.Append("</color>]");
            return _fpsBuilder.ToString();
        }



        private void Update()
        {
            _frameCount++;
            _timer += Time.deltaTime / Time.timeScale;

            if (_timer < measurePeriod)
            {
                return;
            }

            _lastMeasuredFps = (_frameCount / _timer);

            if (Math.Abs(_lastMeasuredFps - _lastFPS) > .001f)
            {
                _fps = _lastMeasuredFps;
                FPSUpdated?.Invoke(_fps);
            }


            _lastFPS = _frameCount;
            _frameCount = 0;

            var rest = measurePeriod - _timer;
            _timer = rest;
        }

        #endregion

        #region --- Vsync ---

        [Monitor]
        [MOrder(1000)]
        [MFontSize(16)]
        [MPosition(UIPosition.UpperRight)]
        [MGroupElement(false)]
        [MValueProcessor(nameof(ProcessorTargetFrameRate))]
        private int TargetFrameRate => Application.targetFrameRate;

        private string ProcessorTargetFrameRate(int value)
        {
            _targetBuilder.Clear();
            _targetBuilder.Append("Target Framerate: ");
            _targetBuilder.Append(value > 0 ? value.ToString() : "Unlimited");
            return _targetBuilder.ToString();
        }

        [Monitor]
        [MFontSize(16)]
        [MPosition(UIPosition.UpperRight)]
        [MGroupElement(false)]
        [MValueProcessor(nameof(ProcessorVsync))]
        [MOrder(1000)]
        private int Vsync => QualitySettings.vSyncCount;

        private string ProcessorVsync(int value)
        {
            _vsyncBuilder.Clear();
            _vsyncBuilder.Append("Vsync: ");
            if (value > 0)
            {
                _vsyncBuilder.Append("Vsync Count: ");
                _vsyncBuilder.Append(value.ToString());
            }
            else
            {
                _vsyncBuilder.Append("Disabled");
            }
            return _vsyncBuilder.ToString();
        }

        #endregion
    }
}