// Copyright (c) 2022 Jonathan Lang

using System.Reflection;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Types;
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
        
        

        internal ValueProcessorFactory(IMonitoringSettings settings)
        {
            _xColor = $"<color=#{ColorUtility.ToHtmlStringRGB(settings.XColor)}>";
            _yColor = $"<color=#{ColorUtility.ToHtmlStringRGB(settings.YColor)}>";
            _zColor = $"<color=#{ColorUtility.ToHtmlStringRGB(settings.ZColor)}>";
            _wColor = $"<color=#{ColorUtility.ToHtmlStringRGB(settings.WColor)}>";

            _trueColored = "TRUE".ColorizeString(settings.TrueColor);
            _falseColored = "FALSE".ColorizeString(settings.FalseColor);
        }
    }
}