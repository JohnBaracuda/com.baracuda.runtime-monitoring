// Copyright (c) 2022 Jonathan Lang

using System;
using System.IO;
using System.Linq;
using Baracuda.Monitoring.Source.Types;
using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.API
{
    public class MonitoringSettings : ScriptableObject, IMonitoringSettings
    {

        #region --- General ---

#pragma warning disable CS0414
        [SerializeField] private bool enableMonitoring = true;

        [Tooltip("When enabled, monitoring UI is instantiated as soon as profiling has completed. " +
                 "Otherwise MonitoringUI.CreateMonitoringUI() must be called manually.")]
        [SerializeField] private bool autoInstantiateUI = true;

        [Tooltip("When enabled, initial profiling will be processed asynchronous on a background thread. (Disabled for WebGL)")]
        [SerializeField] private bool asyncProfiling = true;

        [Tooltip("When enabled, the monitoring display will be opened as soon as profiling has completed.")]
        [SerializeField] private bool openDisplayOnLoad = true;

        [Tooltip("Reference to the used MonitoringDisplay object.")] [SerializeReference, SerializeField]
        private MonitoringUIController monitoringUIController;

        #endregion

        #region --- Debug ---

        [Tooltip("When enabled, the monitoring runtime object is set visible in the hierarchy.")] [SerializeField]
        private bool showRuntimeMonitoringObject = false;

        [Tooltip("BadImageFormatException is a rare exception that may occur during profiling.")] [SerializeField]
        private LoggingLevel logBadImageFormatException = LoggingLevel.None;

        [Tooltip(
            "OperationCanceledException is an exception that may occur when exiting playmode during profiling. It is used to abort the profiling background Task.")]
        [SerializeField]
        private LoggingLevel logOperationCanceledException = LoggingLevel.None;

        [Tooltip(
            "ThreadAbortException may occur if the thread, the profiling is running on is aborted for rare reasons.")]
        [SerializeField]
        private LoggingLevel logThreadAbortException = LoggingLevel.Warning;

        [Tooltip("Set the logging level of every unknown exception that may occur during profiling.")] [SerializeField]
        private LoggingLevel logUnknownExceptions = LoggingLevel.Exception;

        [Tooltip(
            "ProcessorNotFoundException may occur if the passed method name of a ValueProcessor cannot be found. This exception might occur after refactoring. For this reason usage of the nameof keyword is recommended when passing the name of a method.")]
        [SerializeField]
        private LoggingLevel logProcessorNotFoundException = LoggingLevel.Warning;

        [Tooltip(
            "InvalidProcessorSignatureException may occur if a method was passed as a custom ValueProcessor with an invalid signature.")]
        [SerializeField]
        private LoggingLevel logInvalidProcessorSignatureException = LoggingLevel.Warning;

        #endregion

        #region --- Filtering ---
        
        [Tooltip("When enabled, label are used for filtering.")]
        [SerializeField] private bool filterLabel = true;
        [Tooltip("When enabled, static and instance can be used for filtering.")]
        [SerializeField] private bool filterStaticOrInstance = true;
        [Tooltip("When enabled, the type name can be used for filtering. (e.g. int, float, string, etc.)")]
        [SerializeField] private bool filterType = true;
        [Tooltip("When enabled, the members declaring type name can be used for filtering. (e.g. Player, MonoBehaviour etc.)")]
        [SerializeField] private bool filterDeclaringType = true;
        [Tooltip("When enabled, the member type can be used for filtering. (e.g. Field, Property, etc.)")]
        [SerializeField] private bool filterMemberType = true;
        [Tooltip("When enabled, custom tags can be used for filtering.")]
        [SerializeField] private bool filterTags = true;
        [Tooltip("When enabled, you can use the filter 'Interface' to only display monitored interfaces.")]
        [SerializeField] private bool filterInterfaces = true;
        [Tooltip("Set the string comparison used for filtering. Absolute filtering is always case sensitive!")]
        [SerializeField] private StringComparison filterComparison = StringComparison.OrdinalIgnoreCase;
        [Tooltip("Symbol can be used to combine multiple filters.")]
        [SerializeField] private char filterAppendSymbol = '&';
        [Tooltip("Symbol can be used to negate a filter.")]
        [SerializeField] private char filterNegateSymbol = '!';
        [Tooltip("Symbol can be used for absolute filtering, meaning that it is only searching for exact member names.")]
        [SerializeField] private char filterAbsoluteSymbol = '@';
        [Tooltip("Symbol can be used to tag filtering, meaning that it is only searching for custom tags.")]
        [SerializeField] private char filterTagsSymbol = '$';
        
        #endregion
        
        #region --- Formatting ---

        [Tooltip("When enabled, class names will be used as a prefix for displayed units")] [SerializeField]
        private bool addClassName = false;

        [Tooltip("This symbol will be used to separate units class names and their member names.")] [SerializeField]
        private char appendSymbol = '.';

        [Tooltip("When enabled, names of monitored members will be humanized.(e.g. _playerHealth => Player Health)")]
        [SerializeField]
        private bool humanizeNames = true;

        [Tooltip("Collection of variable prefixes that should be removed when humanizing monitored member names")]
        [SerializeField]
        private string[] variablePrefixes = {"m_", "s_", "r_", "_"};

        [Tooltip("Enable the use of RichText on a global level")]
        [SerializeField] 
        private bool enableRichText = true;
        
        #endregion

        #region --- Color ---

        [Header("Color")] 
        [SerializeField] private Color trueColor = Color.green;
        [SerializeField] private Color falseColor = Color.red;

        [Header("Direction Color")] 
        [SerializeField] private Color xColor = new Color(0.41f, 0.38f, 1f);
        [SerializeField] private Color yColor = new Color(0.49f, 1f, 0.53f);
        [SerializeField] private Color zColor = new Color(1f, 0.38f, 0.35f);
        [SerializeField] private Color wColor = new Color(0.6f, 0f, 1f);
        
        [Header("Types")]
        [SerializeField] private Color classColor = new Color(0.49f, 0.49f, 1f);
        [SerializeField] private Color eventColor = new Color(1f, 0.92f, 0.53f);
        [SerializeField] private Color sceneNameColor = new Color(1f, 0.67f, 0.85f);
        [SerializeField] private Color targetObjectColor = new Color(0.39f, 0.72f, 1f);
        [SerializeField] private Color methodColor = new Color(0.56f, 0.98f, 0.53f);
        [SerializeField] private Color outParameterColor = new Color(1f, 0.27f, 0.53f);

        #endregion

        #region --- Assembly Settings ---

        [SerializeField]
        [Tooltip("Assemblies with matching prefixes are ignored when creating a monitoring profile during initialization. Note that Unity, System and other core assemblies will always be ignored.")]
        private string[] bannedAssemblyPrefixes = new string[]
        {
            "Assembly-Plugin",
            "DOTween",
        };

        [SerializeField]
        [Tooltip(
            "Assemblies with matching names are ignored when creating a monitoring profile during initialization. Note that Unity, System and other core assemblies will always be ignored.")]
        private string[] bannedAssemblyNames = new string[]
        {
        };

        #endregion
        
        #region --- IL2CPP ---

        [Tooltip("Reference to the .cs file that will be used to automatically create types for IL2CPP AOT generation, needed in IL2CPP runtime.")]
        [SerializeField] private TextAsset scriptFileIL2CPP;

        [Tooltip("When enabled, this object will listen to an IPreprocessBuildWithReport callback")] 
        [SerializeField] private bool useIPreprocessBuildWithReport = true;

        [Tooltip("When enabled, an exception is thrown if a type cannot be generated by code generation for any reason. This will cancel active build processes.")]
        [SerializeField] private bool throwOnTypeGenerationError = true;

        [Tooltip("The IPreprocessBuildWithReport.callbackOrder of the AOT generation object.")] 
        [SerializeField] private int preprocessBuildCallbackOrder = 0;

        [SerializeField] private bool logTypeGenerationStats = true;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Properties ---

        /*
         * General   
         */
        
        public bool EnableMonitoring => 
#if !DISABLE_MONITORING
            enableMonitoring;
#else
            false;
#endif
        public bool AsyncProfiling =>
#if !UNITY_WEBGL
            asyncProfiling;
#else
            false;
#endif
        
        public bool AutoInstantiateUI => autoInstantiateUI;
        
        /*
         * UI Controller   
         */
        
        public MonitoringUIController UIController => monitoringUIController;
        public bool OpenDisplayOnLoad => openDisplayOnLoad;
        public bool ShowRuntimeMonitoringObject => showRuntimeMonitoringObject;

        /*
         * Logging   
         */
        
        public LoggingLevel LogBadImageFormatException => logBadImageFormatException;
        public LoggingLevel LogOperationCanceledException => logOperationCanceledException;
        public LoggingLevel LogThreadAbortException => logThreadAbortException;
        public LoggingLevel LogUnknownExceptions => logUnknownExceptions;
        public LoggingLevel LogProcessorNotFoundException => logProcessorNotFoundException;
        public LoggingLevel LogInvalidProcessorSignatureException => logInvalidProcessorSignatureException;
        
        /*
         * Filtering
         */
        
        public bool FilterLabel => filterLabel;
        public bool FilterStaticOrInstance => filterStaticOrInstance;
        public bool FilterType => filterType;
        public bool FilterDeclaringType => filterDeclaringType;
        public bool FilterMemberType => filterMemberType;
        public bool FilterTags => filterTags;
        public bool FilterInterfaces => filterInterfaces;
        public StringComparison FilterComparison => filterComparison;
        public char FilterAppendSymbol => filterAppendSymbol;
        public char FilterNegateSymbol => filterNegateSymbol;
        public char FilterAbsoluteSymbol => filterAbsoluteSymbol;
        public char FilterTagsSymbol => filterTagsSymbol;

        /*
         * Formatting   
         */

        public bool AddClassName => addClassName;
        public char AppendSymbol => appendSymbol;
        public bool HumanizeNames => humanizeNames;
        public string[] VariablePrefixes => variablePrefixes;
        public bool RichText => enableRichText;

        /*
         * Coloring   
         */
        
        public Color TrueColor => trueColor;
        public Color FalseColor => falseColor;
        public Color XColor => xColor;
        public Color YColor => yColor;
        public Color ZColor => zColor;
        public Color WColor => wColor;

        public Color MethodColor => methodColor;
        public Color SceneNameColor => sceneNameColor;
        public Color TargetObjectColor => targetObjectColor;
        public Color ClassColor => classColor;
        public Color EventColor => eventColor;
        public Color OutParamColor => outParameterColor;
        
        /*
         * Assembly Settings   
         */
        
        public string[] BannedAssemblyPrefixes => bannedAssemblyPrefixes;
        public string[] BannedAssemblyNames => bannedAssemblyNames;

        /*
         * IL2CPP Settings   
         */

        public TextAsset ScriptFileIL2CPP => scriptFileIL2CPP
            ? scriptFileIL2CPP
            : throw new NullReferenceException(
                "AOT Script file is null! Please create an empty .cs file and assign it in the Monitoring settings!" +
                "\n Window: <b>Tools => Runtime Monitoring => Settings => IL2CPP Settings => Script File IL2CPP</b>");
        public bool UseIPreprocessBuildWithReport => useIPreprocessBuildWithReport;
        public bool ThrowOnTypeGenerationError => throwOnTypeGenerationError;
        public int PreprocessBuildCallbackOrder => preprocessBuildCallbackOrder;
        public bool LogTypeGenerationStats => logTypeGenerationStats;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Asset Logic ---
       
        public static MonitoringSettings FindOrCreateSettingsAsset() =>
            current ? current : current =
                Resources.LoadAll<MonitoringSettings>(string.Empty).FirstOrDefault() ?? CreateAsset() ?? throw new Exception(
                    $"{nameof(ScriptableObject)}: {nameof(MonitoringSettings)} was not found and cannot be created!"); 

        private static MonitoringSettings current;
    
        private static MonitoringSettings CreateAsset()
        {
         var asset = CreateInstance<MonitoringSettings>();
#if UNITY_EDITOR
            var path = $"Assets/Resources";
            Directory.CreateDirectory(path);
            var filePath = $"{path}/{nameof(MonitoringSettings)}.asset";
            Debug.Log($"{nameof(MonitoringSettings)}: Creating new current at path:\n {filePath}");
            UnityEditor.AssetDatabase.CreateAsset(asset, filePath);
            UnityEditor.AssetDatabase.SaveAssets();
#else
            Debug.LogWarning($"Creating new instance of {nameof(MonitoringSettings)}");
#endif
            return asset;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Obsolete ---

        
        [Obsolete("Use IMonitoringSettings instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringSettings>()")]
        public static IMonitoringSettings GetInstance() => MonitoringSystems.Resolve<IMonitoringSettings>();
        
        #endregion
    }
}
