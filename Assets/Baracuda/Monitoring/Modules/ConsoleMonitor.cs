// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Pooling.Concretions;
using Baracuda.Threading;
using UnityEngine;

namespace Baracuda.Monitoring.Modules
{
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

        [SerializeField] [Min(1)] private int displayedMethodNum = 10;
        //[SerializeField] private bool showStacktraceOfLastMessage = true;
        
        #endregion

        #region --- Monitored Values ---
        
        [MonitorProperty]
        [MBackgroundColor(ColorPreset.TransparentBlack)]
        [MUpdateEvent(nameof(UpdateDisplayedLogs))]
        [MFormatOptions(UIPosition.LowerLeft, ShowIndexer = false, ElementIndent = 0, GroupElement = false)]
        [MFont("JetBrainsMono-Regular")]
        private Queue<string> Console => messageLogCache;

        [MonitorProperty]
        [MBackgroundColor(ColorPreset.TransparentBlack)]
        [MFormatOptions(UIPosition.LowerLeft, ShowIndexer = false, Label = "Stacktrace", GroupElement = false)]
        [MFont("JetBrainsMono-Regular")]
        //[MConditional(nameof(showStacktraceOfLastMessage))]
        private string LastLogStacktrace => lastLogStacktrace;

        #endregion

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
            messageCacheSize = displayedMethodNum;
            if (messageLogCache.Count > messageCacheSize)
            {
                messageLogCache.Dequeue();
            }
            
            UpdateDisplayedLogs?.Invoke();
        }

        #region --- Message Log Caching ---
 
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Setup()
        {
            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
        }

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
            sb.Append(condition.Colorize(GetColor(type)));
           
            messageLogCache.Enqueue(ConcurrentStringBuilderPool.Release(sb));
            if (messageLogCache.Count > messageCacheSize)
            {
                messageLogCache.Dequeue();
            }

            lastLogStacktrace = stacktrace.TrimEnd(trimValues).Colorize(stackTraceColor).FontSize(12);
            UpdateDisplayedLogs.Dispatch();
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
