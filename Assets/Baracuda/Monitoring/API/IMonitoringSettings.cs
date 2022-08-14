// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.Source.Types;
using UnityEngine;

namespace Baracuda.Monitoring.API
{
    public interface IMonitoringSettings : IMonitoringSubsystem<IMonitoringSettings>
    {
        bool EnableMonitoring { get; }
        bool AutoInstantiateUI { get; }
        bool AsyncProfiling { get; }
        bool OpenDisplayOnLoad { get; }
        bool ShowRuntimeMonitoringObject { get; }
        LoggingLevel LogBadImageFormatException { get; }
        LoggingLevel LogOperationCanceledException { get; }
        LoggingLevel LogThreadAbortException { get; }
        LoggingLevel LogUnknownExceptions { get; }
        LoggingLevel LogProcessorNotFoundException { get; }
        LoggingLevel LogInvalidProcessorSignatureException { get; }
        bool AddClassName { get; }
        char AppendSymbol { get; }
        bool HumanizeNames { get; }
        string[] VariablePrefixes { get; }
        bool RichText { get; }
        Color TrueColor { get; }
        Color FalseColor { get; }
        Color XColor { get; }
        Color YColor { get; }
        Color ZColor { get; }
        Color WColor { get; }
        Color ClassColor { get; }
        Color EventColor { get; }
        Color SceneNameColor { get; }
        Color TargetObjectColor { get; }
        Color MethodColor { get; }
        string[] BannedAssemblyPrefixes { get; }
        string[] BannedAssemblyNames { get; }
        TextAsset ScriptFileIL2CPP { get; }
        bool UseIPreprocessBuildWithReport { get; }
        bool ThrowOnTypeGenerationError { get; }
        int PreprocessBuildCallbackOrder { get; }
        bool LogTypeGenerationStats { get; }
        MonitoringUIController UIController { get; }
        Color OutParamColor { get; }
        bool FilterLabel { get; }
        bool FilterStaticOrInstance { get; }
        bool FilterType  { get; }
        bool FilterDeclaringType { get; }
        bool FilterMemberType  { get; }
        bool FilterTags  { get; }
        bool FilterInterfaces { get; }
        StringComparison FilterComparison { get; }
        char FilterAppendSymbol { get; }
        char FilterNegateSymbol { get; }
        char FilterAbsoluteSymbol { get; }
        char FilterTagsSymbol { get; }
    }
}