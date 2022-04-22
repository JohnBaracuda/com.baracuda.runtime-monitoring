using System;
using System.IO;
using System.Linq;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Display;
using UnityEngine;
using UnityEngine.UIElements;

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
        [Tooltip("When enabled the game start will be delayed until all profiling has completed. This might increase the startup time significantly!")]
        [SerializeField] private bool forceSynchronousLoad = false;
        [SerializeReference, SerializeField] private MonitoringDisplayHandler monitoringDisplayHandler;

        /*
         * Debugging   
         */
        
        [SerializeField] private LoggingLevel logBadImageFormatException = LoggingLevel.None;
        [SerializeField] private LoggingLevel logOperationCanceledException = LoggingLevel.None;
        [SerializeField] private LoggingLevel logThreadAbortException = LoggingLevel.Warning;
        [SerializeField] private LoggingLevel logUnknownExceptions = LoggingLevel.Exception;
        [SerializeField] private LoggingLevel logBackfieldNotFoundException = LoggingLevel.Warning;
        [SerializeField] private LoggingLevel logProcessorNotFoundException = LoggingLevel.Warning;
        [SerializeField] private LoggingLevel logInvalidProcessorSignatureException = LoggingLevel.Warning;

        /*
         * Formatting   
         */
        
        [Tooltip("When enabled, class names will be used as a prefix for displayed units")]
        [SerializeField] private bool addClassName = true;
        [Tooltip("This symbol will be used to separate units class names and their member names.")]
        [SerializeField] private char appendSymbol = '.';
        [Tooltip("When enabled, static units will be displayed in groups of their declaring type (eg: their class)")]
        [SerializeField] private bool groupStaticUnits = true;
        [Tooltip("When enabled, instance units will be displayed in groups of their target object")]
        [SerializeField] private bool groupInstanceUnits = true;
        [Tooltip("When enabled, names of monitored members will be humanized.(e.g. _playerHealth => Player Health)")]
        [SerializeField] private bool humanizeNames = false;
        [Tooltip("Collection of variable prefixes that should be removed when humanizing monitored member names")]
        [SerializeField] private string[] variablePrefixes = {"m_", "s_", "r_", "_"};

        /*
         * Style   
         */
        
        [SerializeField] private StyleSheet[] optionalStyleSheets;

        [SerializeField] private string instanceUnitStyles = "";
        [SerializeField] private string instanceGroupStyles = "";
        [SerializeField] private string instanceLabelStyles = "";
        
        [SerializeField] private string staticUnitStyles = "";
        [SerializeField] private string staticGroupStyles = "";
        [SerializeField] private string staticLabelStyles = "";

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

        public MonitoringDisplayHandler DisplayDisplayHandler => monitoringDisplayHandler;
        public bool EnableMonitoring => enableMonitoring;
        public bool ForceSynchronousLoad => forceSynchronousLoad;

        
        public LoggingLevel LogBadImageFormatException => logBadImageFormatException;
        public LoggingLevel LogOperationCanceledException => logOperationCanceledException;
        public LoggingLevel LogThreadAbortException => logThreadAbortException;
        public LoggingLevel LogUnknownExceptions => logUnknownExceptions;
        public LoggingLevel LogBackfieldNotFoundException => logBackfieldNotFoundException;
        public LoggingLevel LogProcessorNotFoundException => logProcessorNotFoundException;
        public LoggingLevel LogInvalidProcessorSignatureException => logInvalidProcessorSignatureException;


        public bool AddClassName => addClassName;
        public char AppendSymbol => appendSymbol;
        public bool GroupStaticUnits => groupStaticUnits;
        public bool GroupInstanceUnits => groupInstanceUnits;
        public bool HumanizeNames => humanizeNames;
        public string[] VariablePrefixes => variablePrefixes;
        
        
        public Color ClassColor => classColor;
        public Color TrueColor => trueColor;
        public Color FalseColor => falseColor;
        public Color XColor => xColor;
        public Color YColor => yColor;
        public Color ZColor => zColor;
        public Color WColor => wColor;
        
        
        public StyleSheet[] OptionalStyleSheets => optionalStyleSheets;
        
        
        public string[] BannedAssemblyPrefixes => bannedAssemblyPrefixes;
        public string[] BannedAssemblyNames => bannedAssemblyNames;
        
        public string FilePathIL2CPPTypes => filePathIL2CPPTypes;
        public bool UseIPreprocessBuildWithReport => useIPreprocessBuildWithReport;
        public bool ThrowOnTypeGenerationError => throwOnTypeGenerationError;
        public int PreprocessBuildCallbackOrder => preprocessBuildCallbackOrder;
        
        public string[] InstanceUnitStyles => _instanceUnitStyles ??= instanceUnitStyles.Split(' ');
        public string[] InstanceGroupStyles => _instanceGroupStyles ??= instanceGroupStyles.Split(' ');
        public string[] InstanceLabelStyles => _instanceLabelStyles ??= instanceLabelStyles.Split(' ');
        public string[] StaticUnitStyles => _staticUnitStyles ??= staticUnitStyles.Split(' ');
        public string[] StaticGroupStyles => _staticGroupStyles ??= staticGroupStyles.Split(' ');
        public string[] StaticLabelStyles => _staticLabelStyles ??= staticLabelStyles.Split(' ');
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Type Formatter ---

        [DefaultTypeFormatter(typeof(Vector3))]
        public static string VectorFormat = "0.00";

        [DefaultTypeFormatter(typeof(Quaternion))]
        public static string QuaternionFormat = "0.00";
        
        [DefaultTypeFormatter(typeof(float))]
        public static string FloatFormat = "0.000";
        
        [DefaultTypeFormatter(typeof(double))]
        public static string DoubleFormat = "0.000";

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Singleton ---

        public static MonitoringSettings Instance() =>
            current ? current : current =
                Resources.LoadAll<MonitoringSettings>(string.Empty).FirstOrDefault() ?? CreateAsset() ?? throw new Exception(
                    $"{nameof(ScriptableObject)}: {nameof(MonitoringSettings)} was not found when calling: {nameof(Instance)} and cannot be created!");
        

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
        
        
        
        [NonSerialized] private string[] _instanceUnitStyles = null;
        [NonSerialized] private string[] _instanceGroupStyles = null;
        [NonSerialized] private string[] _instanceLabelStyles = null;

        [NonSerialized] private string[] _staticUnitStyles = null;
        [NonSerialized] private string[] _staticGroupStyles = null;
        [NonSerialized] private string[] _staticLabelStyles = null;
    }
}
