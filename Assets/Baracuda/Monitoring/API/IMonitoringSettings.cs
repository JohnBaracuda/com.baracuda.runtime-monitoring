using Baracuda.Monitoring.Source.Utilities;
using UnityEngine;

namespace Baracuda.Monitoring.API
{
    public interface IMonitoringSettings : IMonitoringSystem<IMonitoringSettings>
    {
        bool EnableMonitoring { get; }
        bool AutoInstantiateUI { get; }
        bool AsyncProfiling { get; }
        bool OpenDisplayOnLoad { get; }
        bool ShowRuntimeMonitoringObject { get; }
        bool ShowRuntimeUIController { get; }
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
        MonitoringUIController UIControllerUIController { get; }
        Color OutParamColor { get; }
    }
}