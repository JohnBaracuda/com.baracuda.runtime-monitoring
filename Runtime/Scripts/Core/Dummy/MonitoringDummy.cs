// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Baracuda.Monitoring.Dummy
{
#pragma warning disable
    internal class MonitoringDummy :
        IMonitoringManager,
        IMonitoringUI,
        IMonitoringUtility,
        IMonitoringSettings,
        IMonitoringRegistry,
        IMonitoringEvents
    {
        #region IMonitoringEvents

        /// <summary>
        /// Event is invoked when profiling process for the current system has been completed.
        /// Subscribing to this event will instantly invoke a callback if profiling has already completed.
        /// </summary>
        public event ProfilingCompletedListener ProfilingCompleted;

        /// <summary>
        /// Event is called when a new <see cref="IMonitorHandle"/> was created.
        /// </summary>
        public event Action<IMonitorHandle> MonitorHandleCreated;

        /// <summary>
        /// Event is called when a <see cref="IMonitorHandle"/> was disposed.
        /// </summary>
        public event Action<IMonitorHandle> MonitorHandleDisposed;


        /// <summary>
        /// Get a list of monitoring units for static targets.
        /// </summary>
        public IReadOnlyList<IMonitorHandle> GetStaticUnits()
        {
            return Array.Empty<IMonitorHandle>();
        }

        /// <summary>
        /// Get a list of monitoring units for instance targets.
        /// </summary>
        public IReadOnlyList<IMonitorHandle> GetInstanceUnits()
        {
            return Array.Empty<IMonitorHandle>();
        }

        /// <summary>
        /// Get a list of all monitoring units.
        /// </summary>
        public IReadOnlyList<IMonitorHandle> GetAllMonitoringUnits()
        {
            return Array.Empty<IMonitorHandle>();
        }

        #endregion


        #region IMonitoringUI

        /// <summary>
        /// Get or set the visibility of the current monitoring UI.
        /// </summary>
        public bool Visible { get; set; } = false;

        /// <summary>
        /// Event is invoked when the monitoring UI became visible/invisible
        /// </summary>
        public event Action<bool> VisibleStateChanged;

        /// <summary>
        /// Get the current monitoring UI instance
        /// </summary>
        public TMonitoringUI GetCurrent<TMonitoringUI>() where TMonitoringUI : MonitoringUI
        {
            return default;
        }

        /// <summary>
        /// ApplyFilter displayed units by their name, tags etc.
        /// </summary>
        public void ApplyFilter(string filter)
        {
        }

        /// <summary>
        /// Reset active filter.
        /// </summary>
        public void ResetFilter()
        {
        }

        /// <summary>
        /// Set the active MonitoringUI
        /// </summary>
        public void SetActiveMonitoringUI(MonitoringUI monitoringUI)
        {
        }

        #endregion


        #region IMonitoringUtility

        /// <summary>
        /// Method returns true if the passed hash from the name of a font asset is used by a MFontNameAttribute and therefore
        /// required by a monitoring unit. Used to dynamically load/unload required fonts.
        /// </summary>
        /// <param name="fontHash">The hash of the fonts name (string)</param>
        public bool IsFontHashUsed(int fontHash)
        {
            return default;
        }

        /// <summary>
        /// Get a list of <see cref="IMonitorHandle"/>s registered to the passed target object.
        /// </summary>
        public IMonitorHandle[] GetMonitorUnitsForTarget(object target)
        {
            return Array.Empty<IMonitorHandle>();
        }

        /// <summary>
        /// Get a list of all custom tags, applied by [MTag] attributes that can be used for filtering.
        /// </summary>
        public IReadOnlyCollection<string> GetAllTags()
        {
            return Array.Empty<string>();
        }

        public IReadOnlyCollection<string> GetAllTypeStrings()
        {
            return Array.Empty<string>();
        }

        #endregion


        #region Settings

        public bool IsMonitoringEnabled { get; } = false;
        public UpdateRate SceneUpdateRate { get; } = UpdateRate.FixedUpdate;
        public bool IsEditorOnly { get; } = false;
        public bool AllowMultipleUIInstances { get; } = false;
        public bool AsyncProfiling { get; } = false;
        public bool UpdatesWithLowTimeScale { get; } = false;
        public bool OpenDisplayOnLoad { get; } = false;
        public bool ShowRuntimeMonitoringObject { get; } = false;
        public LoggingLevel LogBadImageFormatException { get; } = LoggingLevel.None;
        public LoggingLevel LogOperationCanceledException { get; } = LoggingLevel.None;
        public LoggingLevel LogThreadAbortException { get; } = LoggingLevel.None;
        public LoggingLevel LogUnknownExceptions { get; } = LoggingLevel.None;
        public LoggingLevel LogProcessorNotFoundException { get; } = LoggingLevel.None;
        public LoggingLevel LogInvalidProcessorSignatureException { get; } = LoggingLevel.None;
        public bool AddClassName { get; } = false;
        public char AppendSymbol { get; } = default;
        public bool HumanizeNames { get; } = default;
        public string[] VariablePrefixes { get; } = Array.Empty<string>();
        public bool RichText { get; } = false;
        public Color TrueColor { get; } = Color.magenta;
        public Color FalseColor { get; } = Color.magenta;
        public Color XColor { get; } = Color.magenta;
        public Color YColor { get; } = Color.magenta;
        public Color ZColor { get; } = Color.magenta;
        public Color WColor { get; } = Color.magenta;
        public Color ClassColor { get; } = Color.magenta;
        public Color EventColor { get; } = Color.magenta;
        public Color SceneNameColor { get; } = Color.magenta;
        public Color TargetObjectColor { get; } = Color.magenta;
        public Color MethodColor { get; } = Color.magenta;
        public string[] BannedAssemblyPrefixes { get; } = Array.Empty<string>();
        public string[] BannedAssemblyNames { get; } = Array.Empty<string>();
        public TextAsset TypeDefinitionsForIL2CPP { get; } = default;
        public bool UseIPreprocessBuildWithReport { get; } = false;
        public bool ThrowOnTypeGenerationError { get; } = false;
        public int PreprocessBuildCallbackOrder { get; } = 0;

        public MonitoringUI MonitoringUIOverride { get; } = default;
        public Color OutParamColor { get; } = Color.magenta;
        public bool FilterLabel { get; } = false;
        public bool FilterStaticOrInstance { get; } = false;
        public bool FilterType { get; } = false;
        public bool FilterDeclaringType { get; } = false;
        public bool FilterMemberType { get; } = false;
        public bool FilterTags { get; } = false;
        public bool FilterInterfaces { get; } = false;
        public StringComparison FilterComparison { get; } = default;
        public char FilterAppendSymbol { get; } = default;
        public char FilterNegateSymbol { get; } = default;
        public char FilterAbsoluteSymbol { get; } = default;
        public char FilterTagsSymbol { get; } = default;

        #endregion


        #region IMonitoringRegistry

        /// <summary>
        /// Get a list of monitoring handles for all targets.
        /// </summary>
        public IReadOnlyList<IMonitorHandle> GetMonitorHandles(HandleTypes handleTypes = HandleTypes.All)
        {
            return Array.Empty<IMonitorHandle>();
        }

        /// <summary>
        /// Get a list of <see cref="IMonitorHandle"/>s registered to the passed target object.
        /// </summary>
        public IMonitorHandle[] GetMonitorHandlesForTarget<T>(T target) where T : class
        {
            return Array.Empty<IMonitorHandle>();
        }

        /// <summary>
        /// Get a collection of used tags.
        /// </summary>
        public IReadOnlyList<string> UsedTags { get; } = Array.Empty<string>();

        /// <summary>
        /// Get a collection of used fonts.
        /// </summary>
        public IReadOnlyList<string> UsedFonts { get; } = Array.Empty<string>();

        /// <summary>
        /// Get a collection of monitored types.
        /// </summary>
        public IReadOnlyList<Type> UsedTypes { get; } = Array.Empty<Type>();

        /// <summary>
        /// Get a collection of monitored types.
        /// </summary>
        public IReadOnlyList<string> UsedTypeNames { get; } = Array.Empty<string>();

        #endregion


        #region Obsolete

        [Obsolete]
        public bool IsVisible()
        {
            return default;
        }

        [Obsolete]
        public void Show()
        {
        }

        [Obsolete]
        public void Hide()
        {
        }

        [Obsolete]
        public bool ToggleDisplay()
        {
            return default;
        }

        [Obsolete]
        public MonitoringUIController GetActiveUIController()
        {
            return default;
        }

        [Obsolete]
        public TUIController GetActiveUIController<TUIController>() where TUIController : MonitoringUIController
        {
            return default;
        }

        [Obsolete]
        public void CreateMonitoringUI()
        {
        }

        [Obsolete]
        public MonitoringUIController UIController { get; } = default;

        [Obsolete]
        public bool EnableMonitoring { get; } = false;

        [Obsolete]
        public bool AutoInstantiateUI { get; } = false;

        [Obsolete]
        public event Action<IMonitorHandle> UnitCreated;

        [Obsolete]
        public event Action<IMonitorHandle> UnitDisposed;

        [Obsolete]
        public void RegisterTarget<T>(T target) where T : class
        {
        }

        [Obsolete]
        public void UnregisterTarget<T>(T target) where T : class
        {
        }

        [Obsolete]
        public bool IsInitialized { get; } = false;

        [Obsolete]
        event ProfilingCompletedDelegate IMonitoringEvents.ProfilingCompleted
        {
            add { }
            remove { }
        }

        [Obsolete]
        public event ProfilingCompletedListener __ProfilingCompleted;

        #endregion
    }
}