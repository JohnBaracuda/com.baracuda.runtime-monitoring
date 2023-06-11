// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Modules
{
    /// <summary>
    ///     Custom class showcasing how the monitoring system can be used to create a simple console log display.
    /// </summary>
#if MONITORING_EXAMPLES
    [MTag("Console")]
#endif
    public class ConsoleMonitor : MonitorModuleBase
    {
        #region Static

        private static readonly Queue<string> messageLogCache = new Queue<string>(30);
        private static string lastLogStacktrace;

        private static int messageCacheSize = 10;

        private static readonly char[] trimValues = {'\r', '\n'};
        private static Color ErrorColor => new Color(1f, 0.5f, 0.52f);
        private static Color LogColor => new Color(0.8f, 0.75f, 1f);
        private static Color WarningColor => new Color(1f, 0.96f, 0.56f);
        private static Color StackTraceColor => new Color(0.65f, 0.7f, 0.75f);

        private static event Action UpdateDisplayedLogs;

        #endregion


        #region Inspector

        [Header("Display Options")]
        [Min(1)]
        [SerializeField] private int displayedMethodAmount = 10;
        [SerializeField] private bool truncateStacktrace;
        [Range(100, 1000)]
        [SerializeField] private int maxStacktraceLenght = 400;

        #endregion


        #region Monitored Values

        [MonitorProperty]
        [MOrder(-1000)]
        [MBackgroundColor(ColorPreset.TransparentBlack)]
        [MFontName("JetBrainsMono-Regular")]
        [MOptions(UIPosition.LowerRight, ShowIndex = false, ElementIndent = 0, GroupElement = false)]
        [MShowIf(Condition.CollectionNotEmpty)]
        [MUpdateEvent(nameof(UpdateDisplayedLogs))]
        private Queue<string> Console => messageLogCache;

        [Monitor]
        [MOrder(-1001)]
        [MOptions(UIPosition.LowerRight, GroupElement = false)]
        [MFontName("JetBrainsMono-Regular")]
        [MBackgroundColor(ColorPreset.TransparentBlack)]
        [MShowIf(Condition.NotNull)]
        [MUpdateEvent(nameof(UpdateDisplayedLogs))]
        [MValueProcessor(nameof(StacktraceProcessor))]
        private string LastLogStacktrace => lastLogStacktrace;

        #endregion


        #region API

        /// <summary>
        ///     Clear the console display and cache.
        /// </summary>
        public static void Clear()
        {
            messageLogCache.Clear();
            lastLogStacktrace = null;
            UpdateDisplayedLogs?.Invoke();
        }

        #endregion


        #region Event Methods

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            UpdateConfiguration();
        }

        private void OnValidate()
        {
            UpdateConfiguration();
        }

        private void UpdateConfiguration()
        {
            messageCacheSize = displayedMethodAmount;
            if (messageLogCache.Count > messageCacheSize)
            {
                messageLogCache.Dequeue();
            }

            UpdateDisplayedLogs?.Invoke();
        }

        #endregion


        #region Message Log Caching

#if UNITY_WEBGL && !UNITY_EDITOR
        static ConsoleMonitor()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            Application.logMessageReceived += OnLogMessageReceived;
        }
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Setup()
        {
            messageLogCache.Clear();
            lastLogStacktrace = null;
            Application.logMessageReceived -= OnLogMessageReceived;
            Application.logMessageReceived += OnLogMessageReceived;
        }
#endif

        private static readonly StringBuilder logStringBuilder = new StringBuilder();

        private static void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            var sb = logStringBuilder;
            sb.Clear();
            sb.Append('[');
            sb.Append(DateTime.Now.ToString("hh:mm:ss"));
            sb.Append(']');
            sb.Append(' ');
            sb.Append('[');
            sb.Append(AsString(type));
            sb.Append(']');
            sb.Append(' ');
            sb.Append("<color=#");
            sb.Append(ColorUtility.ToHtmlStringRGB(GetColor(type)));
            sb.Append('>');
            sb.Append(condition);
            sb.Append("</color>");

            messageLogCache.Enqueue(sb.ToString());
            if (messageLogCache.Count > messageCacheSize)
            {
                messageLogCache.Dequeue();
            }

            lastLogStacktrace = stacktrace.TrimEnd(trimValues);
            UpdateDisplayedLogs?.Invoke();
        }

        private string StacktraceProcessor(string stacktrace)
        {
            if (stacktrace == null)
            {
                return "<color=#red>null</color>";
            }

            var sb = logStringBuilder;
            sb.Clear();
            sb.Append("Stacktrace:\n");
            if (truncateStacktrace && stacktrace.Length > maxStacktraceLenght)
            {
                sb.Append("<color=#");
                sb.Append(ColorUtility.ToHtmlStringRGB(StackTraceColor));
                sb.Append('>');
                sb.Append(stacktrace.Substring(0, maxStacktraceLenght));
                sb.Append("</color>");
                sb.Append("...");
            }
            else
            {
                sb.Append("<color=#");
                sb.Append(ColorUtility.ToHtmlStringRGB(StackTraceColor));
                sb.Append('>');
                sb.Append(stacktrace);
                sb.Append("</color>");
            }

            return sb.ToString();
        }

        #endregion


        //--------------------------------------------------------------------------------------------------------------


        #region Misc

        private static Color GetColor(LogType logType)
        {
            switch (logType)
            {
                case LogType.Log:
                    return LogColor;
                case LogType.Error:
                    return ErrorColor;
                case LogType.Assert:
                    return WarningColor;
                case LogType.Warning:
                    return WarningColor;
                case LogType.Exception:
                    return ErrorColor;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        private static string AsString(LogType logType)
        {
            switch (logType)
            {
                case LogType.Log:
                    return nameof(LogType.Log);
                case LogType.Error:
                    return nameof(LogType.Error);
                case LogType.Assert:
                    return nameof(LogType.Assert);
                case LogType.Warning:
                    return nameof(LogType.Warning);
                case LogType.Exception:
                    return nameof(LogType.Exception);
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        #endregion
    }
}