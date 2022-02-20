using System;
using System.IO;
using System.Linq;
using Baracuda.Monitoring.Attributes;
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
        
        [Header("Debugging")]
        public LoggingLevel logBadImageFormatException = LoggingLevel.None;
        public LoggingLevel logOperationCanceledException = LoggingLevel.None;
        public LoggingLevel logThreadAbortException = LoggingLevel.Warning;
        public LoggingLevel logUnknownExceptions = LoggingLevel.Exception;
        public LoggingLevel logBackfieldNotFoundException = LoggingLevel.Warning;
        public LoggingLevel logProcessorNotFoundException = LoggingLevel.Warning;
        public LoggingLevel logInvalidProcessorSignatureException = LoggingLevel.Warning;
        
        [Header("Formatting")]
        [Tooltip("When enabled, class names will be used as a prefix for displayed units")]
        public bool addClassName = true;
        [Tooltip("This symbol will be used to separate units class names and their member names.")]
        public char appendSymbol = '.';
        [Tooltip("When enabled, static units will be displayed in groups of their declaring type (eg: their class)")]
        public bool groupStaticUnits = true;
        [Tooltip("When enabled, instance units will be displayed in groups of their target object")]
        public bool groupInstanceUnits = true;
        [Tooltip("When enabled, names of monitored members will be humanized")]
        public bool humanizeNames = false;
        [Tooltip("Collection of variable prefixes that should be removed when humanizing monitored member names")]
        public string[] variablePrefixes = {"m_", "s_", "r_", "_"};

        [Header("Style")] 
        public StyleSheet[] optionalStyleSheets;

        public string instanceUnitStyles = "";
        public string instanceGroupStyles = "";
        public string instanceLabelStyles = "";
        
        public string staticUnitStyles = "";
        public string staticGroupStyles = "";
        public string staticLabelStyles = "";
        
        [Header("Color")]
        public Color classColor = new Color(0.49f, 0.49f, 1f);
        public Color trueColor = Color.green;
        public Color falseColor = Color.gray;
        
        [Header("Direction Color")]
        public Color xColor = new Color(0.41f, 0.38f, 1f);
        public Color yColor = new Color(0.49f, 1f, 0.53f);
        public Color zColor = new Color(1f, 0.38f, 0.35f);
        public Color wColor = new Color(0.6f, 0f, 1f);
        
        
        [Header("Reflection")]
        
        [SerializeField]
        [Tooltip("Assemblies with matching prefixes will not be searched for when creating a monitoring profile during initialization.")]
        private string[] bannedAssemblyPrefixes = new string[]
        {
            "Assembly-Plugin",
            "DOTween",
        };

        [SerializeField]
        [Tooltip("Assemblies with matching names will not be searched for when creating a monitoring profile during initialization.")]
        private string[] bannedAssemblyNames = new string[]
        {
        };
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [PROPERTIES] ---

        public string[] BannedAssemblyPrefixes => bannedAssemblyPrefixes;
        public string[] BannedAssemblyNames => bannedAssemblyNames;

        public string[] InstanceUnitStyles => _mInstanceUnitStyles ??= instanceUnitStyles.Split(' ');
        public string[] InstanceGroupStyles => _mInstanceGroupStyles ??= instanceGroupStyles.Split(' ');
        public string[] InstanceLabelStyles => _mInstanceLabelStyles ??= instanceLabelStyles.Split(' ');
        
        public string[] StaticUnitStyles => _mStaticUnitStyles ??= staticUnitStyles.Split(' ');
        public string[] StaticGroupStyles => _mStaticGroupStyles ??= staticGroupStyles.Split(' ');
        public string[] StaticLabelStyles => _mStaticLabelStyles ??= staticLabelStyles.Split(' ');
        

        [NonSerialized] private string[] _mInstanceUnitStyles = null;
        [NonSerialized] private string[] _mInstanceGroupStyles = null;
        [NonSerialized] private string[] _mInstanceLabelStyles = null;

        [NonSerialized] private string[] _mStaticUnitStyles = null;
        [NonSerialized] private string[] _mStaticGroupStyles = null;
        [NonSerialized] private string[] _mStaticLabelStyles = null;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [TYPE FORMATTER] ---

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
        
        #region --- [SINGLETON] ---

        public static MonitoringSettings Instance() =>
            _current ? _current : _current =
                Resources.LoadAll<MonitoringSettings>(string.Empty).FirstOrDefault() ?? CreateAsset() ?? throw new Exception(
                    $"{nameof(ScriptableObject)}: {nameof(MonitoringSettings)} was not found when calling: {nameof(Instance)} and cannot be created!");
        

        private static MonitoringSettings _current;
    
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
