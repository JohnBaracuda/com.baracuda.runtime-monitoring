// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Interface to access settings of for the monitoring system.
    /// </summary>
    public interface IMonitoringSettings
    {
        /// <summary>
        /// When enabled, the monitoring system is active, otherwise dummy systems are in place to prevent API calls from
        /// resulting in compile or runtime exceptions.
        /// </summary>
        bool IsMonitoringEnabled { get; }

        /// <summary>
        /// When enabled, the monitoring system is active, otherwise dummy systems are in place to prevent API calls from
        /// resulting in compile or runtime exceptions.
        /// </summary>
        UpdateRate SceneUpdateRate { get; }

        /// <summary>
        /// Returns true if runtime monitoring is currently editor only.
        /// </summary>
        bool IsEditorOnly { get; }

        /// <summary>
        /// When enabled, multiple UI instances are allowed simultaneously. Otherwise UI instances are destroyed if a new instance is instantiated / enabled.
        /// </summary>
        bool AllowMultipleUIInstances { get; }

        /// <summary>
        /// When enabled, initial profiling will be processed asynchronous on a background thread. (Disabled for WebGL)
        /// </summary>
        bool AsyncProfiling { get; }

        /// <summary>
        /// When enabled, monitoring is updated even if Time.timeScale is below 0.05
        /// </summary>
        bool UpdatesWithLowTimeScale { get; }

        /// <summary>
        /// When enabled, the monitoring display will be opened as soon as profiling has completed.
        /// </summary>
        bool OpenDisplayOnLoad { get; }

        /// <summary>
        /// When enabled, the monitoring runtime object is set visible in the hierarchy.
        /// </summary>
        bool ShowRuntimeMonitoringObject { get; }

        /// <summary>
        /// BadImageFormatException is a rare exception that may occur during profiling.
        /// </summary>
        LoggingLevel LogBadImageFormatException { get; }

        /// <summary>
        /// OperationCanceledException is an exception that may occur when exiting playmode during profiling.
        /// It is used to abort the profiling background Task.
        /// </summary>
        LoggingLevel LogOperationCanceledException { get; }

        /// <summary>
        /// ThreadAbortException may occur if the thread, the profiling is running on is aborted for rare reasons.
        /// </summary>
        LoggingLevel LogThreadAbortException { get; }

        /// <summary>
        /// Set the logging level of every unknown exception that may occur during profiling.
        /// </summary>
        LoggingLevel LogUnknownExceptions { get; }

        /// <summary>
        /// ProcessorNotFoundException may occur if the passed method name of a ValueProcessor cannot be found.
        /// This exception might occur after refactoring.
        /// For this reason usage of the nameof keyword is recommended when passing the name of a method.
        /// </summary>
        LoggingLevel LogProcessorNotFoundException { get; }

        /// <summary>
        /// InvalidProcessorSignatureException may occur if a method was passed as a custom ValueProcessor with an invalid signature.
        /// </summary>
        LoggingLevel LogInvalidProcessorSignatureException { get; }

        /// <summary>
        /// When enabled, class names will be used as a prefix for displayed units
        /// </summary>
        bool AddClassName { get; }

        /// <summary>
        /// This symbol will be used to separate units class names and their member names.
        /// </summary>
        char AppendSymbol { get; }

        /// <summary>
        /// When enabled, names of monitored members will be humanized.(e.g. _playerHealth => Player Health).
        /// </summary>
        bool HumanizeNames { get; }

        /// <summary>
        /// Collection of variable prefixes that should be removed when humanizing monitored member names.
        /// </summary>
        string[] VariablePrefixes { get; }

        /// <summary>
        /// Enable the use of RichText on a global level.
        /// </summary>
        bool RichText { get; }

        /// <summary>
        /// Color to display the true value of a boolean.
        /// </summary>
        Color TrueColor { get; }

        /// <summary>
        /// Color to display the false value of a boolean.
        /// </summary>
        Color FalseColor { get; }

        /// <summary>
        /// Color to display the true X value of a Vector or Quaternion.
        /// </summary>
        Color XColor { get; }

        /// <summary>
        /// Color to display the true Y value of a Vector or Quaternion.
        /// </summary>
        Color YColor { get; }

        /// <summary>
        /// Color to display the true Z value of a Vector or Quaternion.
        /// </summary>
        Color ZColor { get; }

        /// <summary>
        /// Color to display the true W value of a Vector or Quaternion.
        /// </summary>
        Color WColor { get; }

        /// <summary>
        /// Color to display the name of a class.
        /// </summary>
        Color ClassColor { get; }

        /// <summary>
        /// Color to display the name of an event.
        /// </summary>
        Color EventColor { get; }

        /// <summary>
        /// Color to display the name of a scene.
        /// </summary>
        Color SceneNameColor { get; }

        /// <summary>
        /// Color to display the name of target GameObject.
        /// </summary>
        Color TargetObjectColor { get; }

        /// <summary>
        /// Color to display the name of a method.
        /// </summary>
        Color MethodColor { get; }

        /// <summary>
        /// Color to display the out parameter of a method.
        /// </summary>
        Color OutParamColor { get; }

        /// <summary>
        /// Assemblies with matching prefixes are ignored when creating a monitoring profile during initialization.
        /// Note that Unity, System and other core assemblies will always be ignored.
        /// </summary>
        string[] BannedAssemblyPrefixes { get; }

        /// <summary>
        /// Assemblies with matching names are ignored when creating a monitoring profile during initialization.
        /// Note that Unity, System and other core assemblies will always be ignored.
        /// </summary>
        string[] BannedAssemblyNames { get; }

        /// <summary>
        /// Reference to the .cs file that will be used to automatically create types for IL2CPP AOT generation, needed in IL2CPP runtime.
        /// </summary>
        TextAsset TypeDefinitionsForIL2CPP { get; }

        /// <summary>
        /// When enabled, this object will listen to an IPreprocessBuildWithReport callback
        /// </summary>
        bool UseIPreprocessBuildWithReport { get; }

        /// <summary>
        /// The IPreprocessBuildWithReport.callbackOrder of the AOT generation object.
        /// </summary>
        int PreprocessBuildCallbackOrder { get; }

        /// <summary>
        /// Reference to the an MonitoringUI override.
        /// </summary>
        MonitoringUI MonitoringUIOverride { get; }

        /// <summary>
        /// When enabled, label are used for filtering.
        /// </summary>
        bool FilterLabel { get; }

        /// <summary>
        /// When enabled, static and instance can be used for filtering.
        /// </summary>
        bool FilterStaticOrInstance { get; }

        /// <summary>
        /// When enabled, the type name can be used for filtering. (e.g. int, float, string, etc.)
        /// </summary>
        bool FilterType { get; }

        /// <summary>
        /// When enabled, the members declaring type name can be used for filtering. (e.g. Player, MonoBehaviour etc.)
        /// </summary>
        bool FilterDeclaringType { get; }

        /// <summary>
        /// When enabled, the member type can be used for filtering. (e.g. Field, Property, etc.)
        /// </summary>
        bool FilterMemberType { get; }

        /// <summary>
        /// When enabled, custom tags can be used for filtering.
        /// </summary>
        bool FilterTags { get; }

        /// <summary>
        /// When enabled, you can use the filter 'Interface' to only display monitored interfaces.
        /// </summary>
        bool FilterInterfaces { get; }

        /// <summary>
        /// Set the string comparison used for filtering. Absolute filtering is always case sensitive!
        /// </summary>
        StringComparison FilterComparison { get; }

        /// <summary>
        /// Symbol can be used to combine multiple filters.
        /// </summary>
        char FilterAppendSymbol { get; }

        /// <summary>
        /// Symbol can be used to negate a filter.
        /// </summary>
        char FilterNegateSymbol { get; }

        /// <summary>
        /// Symbol can be used for absolute filtering, meaning that it is only searching for exact member names.
        /// </summary>
        char FilterAbsoluteSymbol { get; }

        /// <summary>
        /// Symbol can be used to tag filtering, meaning that it is only searching for custom tags.
        /// </summary>
        char FilterTagsSymbol { get; }

        //--------------------------------------------------------------------------------------------------------------

        #region Obsolete

        [Obsolete("Use MonitoringUIOverride instead! This API will be removed in 4.0.0")]
        MonitoringUIController UIController { get; }

        [Obsolete("Use IMonitoringSettings.IsMonitoringEnabled instead. This API will be removed in 4.0.0")]
        bool EnableMonitoring { get; }

        [Obsolete("This API will be removed in 4.0.0")]
        bool AutoInstantiateUI { get; }

        #endregion
    }
}