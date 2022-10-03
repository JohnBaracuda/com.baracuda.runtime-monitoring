// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Baracuda.Monitoring.Dummy
{
    internal class MonitoringSystemDummy :
        IMonitoringManager,
        IMonitoringUI,
        IMonitoringUtility,
        IMonitoringSettings
    {
        #region --- Pragma ---
#pragma warning disable
        #endregion

        #region --- MonitoringManager ---

        /// <summary>
        /// Value indicated whether or not monitoring profiling has completed and monitoring is fully initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public bool IsInitialized { get; } = false;

        /// <summary>
        /// Event is invoked when profiling process for the current system has been completed.
        /// Subscribing to this event will instantly invoke a callback if profiling has already completed.
        /// </summary>
        public event ProfilingCompletedListener ProfilingCompleted;

        /// <summary>
        /// Event is called when a new <see cref="MonitorUnit"/> was created.
        /// </summary>
        public event Action<IMonitorUnit> UnitCreated;

        /// <summary>
        /// Event is called when a <see cref="MonitorUnit"/> was disposed.
        /// </summary>
        public event Action<IMonitorUnit> UnitDisposed;

        /// <summary>
        /// Register an object that is monitored during runtime.
        /// </summary>
        public void RegisterTarget<T>(T target) where T : class
        {
        }

        /// <summary>
        /// Unregister an object that is monitored during runtime.
        /// </summary>
        public void UnregisterTarget<T>(T target) where T : class
        {
        }

        /// <summary>
        /// Get a list of monitoring units for static targets.
        /// </summary>
        public IReadOnlyList<IMonitorUnit> GetStaticUnits()
        {
            return Array.Empty<IMonitorUnit>();
        }

        /// <summary>
        /// Get a list of monitoring units for instance targets.
        /// </summary>
        public IReadOnlyList<IMonitorUnit> GetInstanceUnits()
        {
            return Array.Empty<IMonitorUnit>();
        }

        /// <summary>
        /// Get a list of all monitoring units.
        /// </summary>
        public IReadOnlyList<IMonitorUnit> GetAllMonitoringUnits()
        {
            return Array.Empty<IMonitorUnit>();
        }

        #endregion

        #region --- IMonitoringUI ---

        /// <summary>
        /// Set the active monitoring display visible.
        /// </summary>
        public void Show()
        {
        }

        /// <summary>
        /// Hide the active monitoring display.
        /// </summary>
        public void Hide()
        {
        }

        /// <summary>
        /// Toggle the visibility of the active monitoring display.
        /// This method returns a value indicating the new visibility state.
        /// </summary>
        public bool ToggleDisplay()
        {
            return default;
        }

        /// <summary>
        /// Event is invoked when the monitoring UI became visible/invisible
        /// </summary>
        public event Action<bool> VisibleStateChanged;

        /// <summary>
        /// Returns true if the there is an active monitoring display that is also visible.
        /// </summary>
        public bool IsVisible()
        {
            return default;
        }

        /// <summary>
        /// Get the current <see cref="MonitoringUIController"/>
        /// </summary>
        public MonitoringUIController GetActiveUIController()
        {
            return default;
        }

        /// <summary>
        /// Get the current <see cref="MonitoringUIController"/> as a concrete implementation of T.
        /// </summary>
        public TUIController GetActiveUIController<TUIController>() where TUIController : MonitoringUIController
        {
            return default;
        }

        /// <summary>
        /// Create a MonitoringUIController instance if there is none already. Disable 'Auto Instantiate UI' in the
        /// Monitoring Settings and use this method for more control over the timing in which the MonitoringUIController
        /// is instantiated.
        /// </summary>
        public void CreateMonitoringUI()
        {
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

        public void Filter(string filter)
        {
        }

        public void Initialize()
        {

        }

        #endregion

        #region --- IMonitoringUtility ---

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
        /// Get a list of <see cref="IMonitorUnit"/>s registered to the passed target object.
        /// </summary>
        public IMonitorUnit[] GetMonitorUnitsForTarget(object target)
        {
            return Array.Empty<IMonitorUnit>();
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

        #region --- Settings ---

        public bool EnableMonitoring { get; } = false;
        public bool AutoInstantiateUI { get; } = false;
        public bool AsyncProfiling { get; } = false;
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
        public MonitoringUIController UIController { get; } = default;
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
    }
}