// Copyright (c) 2022 Jonathan Lang
using System;
using System.IO;
using System.Linq;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.API
{
    public class MonitoringSettings : ScriptableObject
    {
        #region --- General ---

#pragma warning disable CS0414
        [SerializeField] private bool enableMonitoring = true;

        [Tooltip("When enabled, monitoring UI is instantiated as soon as profiling has completed. " +
                 "Otherwise MonitoringUI.CreateMonitoringUI() must be called manually.")]
        [SerializeField]
        private bool autoInstantiateUI = false;

        [Tooltip(
            "When enabled, initial profiling will be processed asynchronous on a background thread. (Disabled for WebGL)")]
        [SerializeField]
        private bool asyncProfiling = true;

        [Tooltip("When enabled, the monitoring display will be opened as soon as profiling has completed.")]
        [SerializeField]
        private bool openDisplayOnLoad = true;

        [Tooltip("Reference to the used MonitoringDisplay object.")] [SerializeReference, SerializeField]
        private MonitoringUIController monitoringUIController;

        #endregion

        #region --- Debug ---

        [Tooltip("When enabled, the monitoring runtime object is set visible in the hierarchy.")] [SerializeField]
        private bool showRuntimeMonitoringObject = false;

        [Tooltip("When enabled, the monitoring UI Controller object is set visible in the hierarchy.")] [SerializeField]
        private bool showRuntimeUIController = false;

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

        #region --- Formatting ---

        [Tooltip("When enabled, class names will be used as a prefix for displayed units")] [SerializeField]
        private bool addClassName = true;

        [Tooltip("This symbol will be used to separate units class names and their member names.")] [SerializeField]
        private char appendSymbol = '.';

        [Tooltip("When enabled, names of monitored members will be humanized.(e.g. _playerHealth => Player Health)")]
        [SerializeField]
        private bool humanizeNames = false;

        [Tooltip("Collection of variable prefixes that should be removed when humanizing monitored member names")]
        [SerializeField]
        private string[] variablePrefixes = {"m_", "s_", "r_", "_"};
        
        #endregion

        #region --- Color ---

        [Header("Color")] 
        [SerializeField] private Color trueColor = Color.green;
        [SerializeField] private Color falseColor = Color.gray;

        [Header("Direction Color")] 
        [SerializeField] private Color xColor = new Color(0.41f, 0.38f, 1f);
        [SerializeField] private Color yColor = new Color(0.49f, 1f, 0.53f);
        [SerializeField] private Color zColor = new Color(1f, 0.38f, 0.35f);
        [SerializeField] private Color wColor = new Color(0.6f, 0f, 1f);
        
        [Header("Types")]
        [SerializeField] private Color classColor = new Color(0.49f, 0.49f, 1f);
        [SerializeField] private Color eventColor = new Color(1f, 0.92f, 0.53f);

        #endregion

        #region --- Assembly Settings ---

        [SerializeField]
        [Tooltip(
            "Assemblies with matching prefixes are ignored when creating a monitoring profile during initialization. Note that Unity, System and other core assemblies will always be ignored.")]
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

        [Tooltip(
            "Filepath to a custom .cs file that will be used to automatically create types for IL2CPP AOT generation, needed in IL2CPP runtime.")]
        [SerializeField]
        private string filePathIL2CPPTypes = string.Empty;

        [Tooltip("When enabled, this object will listen to an IPreprocessBuildWithReport callback")] [SerializeField]
        private bool useIPreprocessBuildWithReport = true;

        [Tooltip(
            "When enabled, an exception is thrown if a type cannot be generated by code generation for any reason. This will cancel active build processes.")]
        [SerializeField]
        private bool throwOnTypeGenerationError = true;

        [Tooltip("The IPreprocessBuildWithReport.callbackOrder of the AOT generation object.")] [SerializeField]
        private int preprocessBuildCallbackOrder = 0;

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
        
        public MonitoringUIController UIControllerUIController => monitoringUIController;
        public bool OpenDisplayOnLoad => openDisplayOnLoad;
        public bool ShowRuntimeMonitoringObject => showRuntimeMonitoringObject;
        public bool ShowRuntimeUIController => showRuntimeUIController;

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
         * Formatting   
         */

        public bool AddClassName => addClassName;
        public char AppendSymbol => appendSymbol;
        public bool HumanizeNames => humanizeNames;
        public string[] VariablePrefixes => variablePrefixes;

        /*
         * Coloring   
         */
        
        public Color TrueColor => trueColor;
        public Color FalseColor => falseColor;
        public Color XColor => xColor;
        public Color YColor => yColor;
        public Color ZColor => zColor;
        public Color WColor => wColor;
        
        public Color ClassColor => classColor;
        public Color EventColor => eventColor;

        /*
         * Assembly Settings   
         */
        
        public string[] BannedAssemblyPrefixes => bannedAssemblyPrefixes;
        public string[] BannedAssemblyNames => bannedAssemblyNames;

        /*
         * IL2CPP Settings   
         */
        
        public string FilePathIL2CPPTypes => filePathIL2CPPTypes;
        public bool UseIPreprocessBuildWithReport => useIPreprocessBuildWithReport;
        public bool ThrowOnTypeGenerationError => throwOnTypeGenerationError;
        public int PreprocessBuildCallbackOrder => preprocessBuildCallbackOrder;

        /*
         * Const   
         */

        public const string COPYRIGHT = "© 2022 Jonathan Lang";
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Singleton ---
        
        public static MonitoringSettings GetInstance() =>
            current ? current : current =
                Resources.LoadAll<MonitoringSettings>(string.Empty).FirstOrDefault() ?? CreateAsset() ?? throw new Exception(
                    $"{nameof(ScriptableObject)}: {nameof(MonitoringSettings)} was not found when calling: {nameof(GetInstance)} and cannot be created!");
        

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
    }
}
