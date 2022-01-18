using System;
using System.IO;
using System.Linq;
using Baracuda.Threading.Internal;
using UnityEngine;
using UnityEditor;

namespace Baracuda.Threading.Editor
{
    internal class DispatcherExecutionOrder : ScriptableObject
    {
        #region --- [FIELDS] ---

        [SerializeField][HideInInspector] 
        internal int executionOrder = DEFAULT_EXECUTION_ORDER;
        
        [SerializeField][HideInInspector] 
        internal int postExecutionOrder = DEFAULT_POST_EXECUTION_ORDER;
       
        private static DispatcherExecutionOrder _current;
        private const string PATH = "Assets/Baracuda/Threading/Editor/Resources";
        internal const int DEFAULT_EXECUTION_ORDER = -500;
        internal const int DEFAULT_POST_EXECUTION_ORDER = 500;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [SINGLETON LOGIC] ---

        internal static DispatcherExecutionOrder GetConfigurationFile()
        {
            if (_current) return _current;
            return _current = Resources.LoadAll<DispatcherExecutionOrder>(string.Empty).FirstOrDefault() ?? CreateDispatcherCache() ?? throw new Exception(
                $"{nameof(DispatcherExecutionOrder)} was not found when calling: {nameof(GetConfigurationFile)} and cannot be created!");
        }
        
        private static DispatcherExecutionOrder CreateDispatcherCache()
        {
            try
            {
                Directory.CreateDirectory(PATH);
                var filePath = $"{PATH}/Execution-Config.asset";

                var asset = CreateInstance<DispatcherExecutionOrder>();
                AssetDatabase.CreateAsset(asset, filePath);
                AssetDatabase.SaveAssets();
                
#if DISPATCHER_DEBUG
                Debug.Log($"Created a new {nameof(DispatcherExecutionOrder)} at {PATH}! ");
#endif
                return asset;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return null;
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [EXECUTION ORDER LOGIC] ---

        /// <summary>
        /// Validate and update the current script execution order of the <see cref="Dispatcher"/>.
        /// </summary>
        [InitializeOnLoadMethod]
        public static void ValidateExecutionOrder()
        {
            var target = GetConfigurationFile();
            var monoScripts = MonoImporter.GetAllRuntimeMonoScripts();
            for (var i = 0; i < monoScripts.Length; i++)
            {
                UpdateExecutionOrderForType<Dispatcher>(monoScripts[i], target.executionOrder);
                UpdateExecutionOrderForType<DispatcherPostUpdate>(monoScripts[i], target.postExecutionOrder);
            }
        }

        private static void UpdateExecutionOrderForType<T>(MonoScript target, int newOrder) where T : MonoBehaviour
        {
            if (target.GetClass() != typeof(T))
            {
                return;
            }
            
            var currentOrder = MonoImporter.GetExecutionOrder(target);

            if (currentOrder == newOrder)
            {
                return;
            }
            
            Debug.Log($"Setting the 'Script Execution Order' of {target.name} from {currentOrder} to {newOrder}");
            MonoImporter.SetExecutionOrder(target, newOrder);
        }

        #endregion
    }
    
    internal class ExecutionOrderWindow : EditorWindow
    {
        #region --- [FIELDS] ---

        private DispatcherExecutionOrder _target = null;
        
        private readonly GUIContent _executionOrderContent = new GUIContent("Main Execution Order", 
            "Set the script execution order for the Dispatcher.");
        
        private readonly GUIContent _postExecutionOrderContent = new GUIContent("Post Update Order", 
            "Set the script execution order for the post update call of the Dispatcher.");

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [SETUP] ---

        internal static void Open()
        {
            var window = GetWindow<ExecutionOrderWindow>("Execution Order");
            window._target = DispatcherExecutionOrder.GetConfigurationFile();
            window.Show(true);
        }

        private void OnDisable()
        {
            DispatcherExecutionOrder.ValidateExecutionOrder();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [GUI] ---

        public void OnGUI()
        {
            var style = GUI.skin.GetStyle("HelpBox");
            style.fontSize = 12;
            style.richText = true;
            EditorGUILayout.TextArea("The <b>'Dispatcher Execution Order'</b> affects the script execution order of the Dispatchers Update, LateUpdate and FixedUpdate." +
                                     "The <b>'Post Update Execution Order'</b> affects the script execution order of the Dispatchers PostUpdate." +
                                     "Post Update is technically another LateUpdate call, that is invoked on another component and then forwarded to the Dispatcher.", style);
            
            GUILayout.Space(5);
            
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Execution Order");
            _target.executionOrder = EditorGUILayout.IntField(_executionOrderContent, _target.executionOrder);
            _target.postExecutionOrder = EditorGUILayout.IntField(_postExecutionOrderContent, _target.postExecutionOrder);
            GUILayout.EndVertical();

            if (_target.executionOrder >= 0)
            {
                EditorGUILayout.HelpBox("Dispatcher Execution Order should be less then 0 to ensure that the Dispatcher is called before the default execution time.", MessageType.Warning);
            }

            if (_target.postExecutionOrder <= 0)
            {
                EditorGUILayout.HelpBox("Post Update Execution Order should be greater then 0 to ensure that the Post Update is called after the default execution time.", MessageType.Warning);
            }

            if (_target.executionOrder > _target.postExecutionOrder)
            {
                EditorGUILayout.HelpBox("Dispatcher Execution Order should be less then Post Update Execution Order to ensure that the Dispatchers PostUpdate is called after the the Dispatchers LateUpdate!", MessageType.Warning);
            }
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Documentation", GUILayout.MinWidth(120)))
            {
                Application.OpenURL("https://johnbaracuda.com/dispatcher.html#execution-order");
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset", GUILayout.MinWidth(120)))
            {
                _target.executionOrder = DispatcherExecutionOrder.DEFAULT_EXECUTION_ORDER;
                _target.postExecutionOrder = DispatcherExecutionOrder.DEFAULT_POST_EXECUTION_ORDER;
            }
            if (GUILayout.Button("Save", GUILayout.MinWidth(120)))
            {
                DispatcherExecutionOrder.ValidateExecutionOrder();
            }
            GUILayout.EndHorizontal();

            EditorUtility.SetDirty(_target);
        }

        #endregion
    }
}