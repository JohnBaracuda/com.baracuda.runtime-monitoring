// Copyright (c) 2022 Jonathan Lang

using System.Reflection;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Utilities;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Systems
{
    internal partial class ValueProcessorFactory
    {
        /*
         * Fields   
         */
        
        private const string DEFAULT_INDENT = "  ";
        private const int DEFAULT_INDENT_NUM = 2;
        private const string NULL = "<color=red>NULL</color>";
        private readonly string _xColor;
        private readonly string _yColor;
        private readonly string _zColor;
        private readonly string _wColor;
        private readonly string _trueColored;
        private readonly string _falseColored;

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
        
        

        internal ValueProcessorFactory()
        {
            var settings = MonitoringSystems.Resolve<IMonitoringSettings>();
            _xColor = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.XColor)}>";
            _yColor = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.YColor)}>";
            _zColor = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.ZColor)}>";
            _wColor = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.WColor)}>";

            _trueColored = "TRUE".Colorize(settings.TrueColor);
            _falseColored = "FALSE".Colorize(settings.FalseColor);
        }
    }
}