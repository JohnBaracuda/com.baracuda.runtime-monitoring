using System;
using System.IO;
using System.Linq;
using Baracuda.Monitoring.Display;
using Baracuda.Monitoring.Internal.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Management
{
    public enum LoggingLevel
    {
        None = 0,
        Message,
        Warning,
        Error,
        Exception
    }
    
    public class MonitoringSettings : ScriptableObject
    {
        /*
         * General   
         */

        [SerializeField] private bool enableMonitoring = true;
        [SerializeField] private bool openDisplayOnLoad = true;
        [SerializeReference, SerializeField] private MonitoringDisplay monitoringDisplay;

        [Tooltip("When enabled the game start will be delayed until all profiling has completed. This might increase the startup time significantly!")]
        [SerializeField] private bool forceSynchronousLoad = false;

        [Space] 
        [SerializeField] private bool showRuntimeObject = false;

        /*
         * Debugging   
         */
        
        [SerializeField] private LoggingLevel logBadImageFormatException = LoggingLevel.None;
        [SerializeField] private LoggingLevel logOperationCanceledException = LoggingLevel.None;
        [SerializeField] private LoggingLevel logThreadAbortException = LoggingLevel.Warning;
        [SerializeField] private LoggingLevel logUnknownExceptions = LoggingLevel.Exception;
        [SerializeField] private LoggingLevel logProcessorNotFoundException = LoggingLevel.Warning;
        [SerializeField] private LoggingLevel logInvalidProcessorSignatureException = LoggingLevel.Warning;

        /*
         * Formatting   
         */
        
        [Tooltip("When enabled, class names will be used as a prefix for displayed units")]
        [SerializeField] private bool addClassName = true;
        [Tooltip("This symbol will be used to separate units class names and their member names.")]
        [SerializeField] private char appendSymbol = '.';
        [Tooltip("When enabled, names of monitored members will be humanized.(e.g. _playerHealth => Player Health)")]
        [SerializeField] private bool humanizeNames = false;
        [Tooltip("Collection of variable prefixes that should be removed when humanizing monitored member names")]
        [SerializeField] private string[] variablePrefixes = {"m_", "s_", "r_", "_"};

        [Header("Type Formatter")] 
        [Tooltip("Default formatting string that is applied to every float. (float, double)")]
        [SerializeField] private string floatFormat = "0.000";
        [Tooltip("Default formatting string that is applied to every integer type. (short, int, long)")]
        [SerializeField] private string integerFormat = "0";
        [Tooltip("Default formatting string that is applied to the individual values of every Vector type. (Vector2, Vector3)")]
        [SerializeField] private string vectorFormat = "0.00";
        [Tooltip("Default formatting string that is applied to the individual values of every Quaternion")]
        [SerializeField] private string quaternionFormat = "0.00";

        /*
         * Color   
         */

        [Header("Color")]
        [SerializeField] private Color classColor = new Color(0.49f, 0.49f, 1f);
        [SerializeField] private Color trueColor = Color.green;
        [SerializeField] private Color falseColor = Color.gray;
        
        [Header("Direction Color")]
        [SerializeField] private Color xColor = new Color(0.41f, 0.38f, 1f);
        [SerializeField] private Color yColor = new Color(0.49f, 1f, 0.53f);
        [SerializeField] private Color zColor = new Color(1f, 0.38f, 0.35f);
        [SerializeField] private Color wColor = new Color(0.6f, 0f, 1f);
        
        /*
         * Assembly Settings   
         */
        
        [SerializeField]
        [Tooltip("Assemblies with matching prefixes are ignored when creating a monitoring profile during initialization.")]
        private string[] bannedAssemblyPrefixes = new string[]
        {
            "Assembly-Plugin",
            "DOTween",
        };

        [SerializeField]
        [Tooltip("Assemblies with matching names are ignored when creating a monitoring profile during initialization.")]
        private string[] bannedAssemblyNames = new string[]
        {
        };

        /*
         * IL2CPP Settings   
         */
        
        [SerializeField] private string filePathIL2CPPTypes = string.Empty;
        [Tooltip("When enabled, this object will listen to an IPreprocessBuildWithReport callback")]
        [SerializeField] private bool useIPreprocessBuildWithReport = true;
        [SerializeField] private bool throwOnTypeGenerationError = false;
        [SerializeField] private int preprocessBuildCallbackOrder = 0;
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Properties ---

        /*
         * General   
         */
        
        public MonitoringDisplay DisplayDisplay => monitoringDisplay;
        public bool EnableMonitoring => enableMonitoring;
        public bool OpenDisplayOnLoad => openDisplayOnLoad;
        public bool ShowRuntimeObject => showRuntimeObject;
        public bool ForceSynchronousLoad => forceSynchronousLoad;

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
        
        public Color ClassColor => classColor;
        public Color TrueColor => trueColor;
        public Color FalseColor => falseColor;
        public Color XColor => xColor;
        public Color YColor => yColor;
        public Color ZColor => zColor;
        public Color WColor => wColor;

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

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Type Formatter ---

        internal string GetFormatStringForType(Type type)
        {
            if (type.IsFloatingPoint())
            {
                return floatFormat;
            }

            if (type.IsVector())
            {
                return vectorFormat;
            }

            if (type.IsInteger())
            {
                return integerFormat;
            }

            if (type == typeof(Quaternion))
            {
                return quaternionFormat;
            }

            return null;
        }

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
