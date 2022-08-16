// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using Baracuda.Monitoring.Source.Types;
using Baracuda.Threading;
using Baracuda.Utilities.Pooling;
using UnityEngine;

namespace Baracuda.Monitoring.Modules
{
    [MTag("Console")]
    public class ConsoleMonitor : MonitorModuleBase
    {
        #region --- Static ---

        private static readonly Queue<string> messageLogCache = new Queue<string>(30);
        private static string lastLogStacktrace;

        private static int messageCacheSize = 10;
        
        private static readonly char[] trimValues = {'\r', '\n'};
        private static readonly Color errorColor = new Color(1f, 0.5f, 0.52f);
        private static readonly Color logColor = new Color(0.8f, 0.75f, 1f);
        private static readonly Color warningColor = new Color(1f, 0.96f, 0.56f);
        private static readonly Color stackTraceColor = new Color(0.65f, 0.7f, 0.75f);
        
        private static event Action UpdateDisplayedLogs;
        
        #endregion
        
        #region --- Inspector ---

        [Header("Display Options")]
        [Min(1)]
        [SerializeField] private int displayedMethodAmount = 10;
        [SerializeField] private bool truncateStacktrace = false;
        [Range(100,1000)]
        [SerializeField] private int maxStacktraceLenght = 400;
        
        #endregion
        
        #region --- Monitored Values ---
        
        [MonitorProperty]
        [MOrder(-1000)]
        [MBackgroundColor(ColorPreset.TransparentBlack)]
        [MFontName("JetBrainsMono-Regular")]
        [MOptions(UIPosition.LowerRight, ShowIndexer = false, ElementIndent = 0, GroupElement = false)]
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

        #region --- API ---

        public static void Clear()
        {
            messageLogCache.Clear();
            lastLogStacktrace = null;
            UpdateDisplayedLogs?.Invoke();
        }
        
        #endregion
        
        #region --- Event Methods ---

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

        #region --- Message Log Caching ---
 
#if UNITY_WEBGL
        static ConsoleMonitor()
        {
            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
        }
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Setup()
        {
            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
        }
#endif

        private static void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            var sb = ConcurrentStringBuilderPool.Get();
            
            sb.Append('[');
            sb.Append(DateTime.Now.ToString("hh:mm:ss"));
            sb.Append(']');
            sb.Append(' ');
            sb.Append('[');
            sb.Append(AsString(type));
            sb.Append(']');
            sb.Append(' ');
            sb.Append(condition.ColorizeString(GetColor(type)));
           
            messageLogCache.Enqueue(ConcurrentStringBuilderPool.Release(sb));
            if (messageLogCache.Count > messageCacheSize)
            {
                messageLogCache.Dequeue();
            }

            lastLogStacktrace = stacktrace.TrimEnd(trimValues);
            UpdateDisplayedLogs.Dispatch();
        }
        
        private string StacktraceProcessor(string stacktrace)
        {
            if (stacktrace == null)
            {
                return "null".ColorizeString(Color.red);
            }

            var sb = StringBuilderPool.Get();
            sb.Append("Stacktrace:\n");
            if (truncateStacktrace && stacktrace.Length > maxStacktraceLenght)
            {
                sb.Append(stacktrace.Substring(0, maxStacktraceLenght).ColorizeString(stackTraceColor));
                sb.Append("...");
            }
            else
            {
                sb.Append(stacktrace.ColorizeString(stackTraceColor));
            }

            return StringBuilderPool.Release(sb);
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Misc ---

        private static Color GetColor(LogType logType)
        {
            switch (logType)
            {
                case LogType.Log:
                    return logColor;
                case LogType.Error:
                    return errorColor;
                case LogType.Assert:
                    return warningColor;
                case LogType.Warning:
                    return warningColor;
                case LogType.Exception:
                    return errorColor;
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
