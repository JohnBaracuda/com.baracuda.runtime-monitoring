// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using System.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Systems
{
    internal partial class ValueProcessorFactory
    {
        /*
         * Fields
         */

        private const string DefaultIndent = "  ";
        private const int DefaultIndentNum = 2;
        private const string Null = "null";
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
            var settings = Monitor.Settings;

            _xColor = $"<color=#{ColorUtility.ToHtmlStringRGB(settings.XColor)}>";
            _yColor = $"<color=#{ColorUtility.ToHtmlStringRGB(settings.YColor)}>";
            _zColor = $"<color=#{ColorUtility.ToHtmlStringRGB(settings.ZColor)}>";
            _wColor = $"<color=#{ColorUtility.ToHtmlStringRGB(settings.WColor)}>";

            _trueColored = "TRUE".ColorizeString(settings.TrueColor);
            _falseColored = "FALSE".ColorizeString(settings.FalseColor);
        }
    }
}