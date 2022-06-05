// Copyright (c) 2022 Jonathan Lang
using System.Reflection;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Internal.Utilities;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Profiling
{
    internal static partial class ValueProcessorFactory
    {
        /*
         * Fields   
         */

        private const string DEFAULT_INDENT = "  ";
        private const int DEFAULT_INDENT_NUM = 2;
        private const string NULL = "<color=red>NULL</color>";
        private static string xColor;
        private static string yColor;
        private static string zColor;
        private static string wColor;
        private static string trueColored;
        private static string falseColored;

        /*
         * Const   
         */

        private const BindingFlags FLAGS
            = BindingFlags.Default |
              BindingFlags.Static |
              BindingFlags.Public |
              BindingFlags.NonPublic |
              BindingFlags.DeclaredOnly |
              BindingFlags.Instance;

        private const BindingFlags STATIC_FLAGS
            = BindingFlags.Default |
              BindingFlags.Static |
              BindingFlags.Public |
              BindingFlags.NonPublic |
              BindingFlags.DeclaredOnly;

        private const BindingFlags INSTANCE_FLAGS
            = BindingFlags.Instance |
              BindingFlags.NonPublic |
              BindingFlags.Public |
              BindingFlags.DeclaredOnly;

        /*
         * Setup   
         */

        internal static void Initialize(MonitoringSettings settings)
        {
            xColor = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.XColor)}>";
            yColor = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.YColor)}>";
            zColor = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.ZColor)}>";
            wColor = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.WColor)}>";

            trueColored = "TRUE".Colorize(settings.TrueColor);
            falseColored = "FALSE".Colorize(settings.FalseColor);
        }
    }
}