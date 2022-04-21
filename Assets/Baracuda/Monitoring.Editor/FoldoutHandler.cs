using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Monitoring.Editor
{
    public class FoldoutHandler
    {
        /*
         * Properties    
         */
        public Dictionary<string, bool> Data => _data;
        public Dictionary<string, bool> DefaultFoldoutStates { get; } = new Dictionary<string, bool>();

        /*
         * Fields   
         */
        
        private readonly Dictionary<string, bool> _data;
        private readonly bool _indent;
        private readonly string _dataKey;

        /*
         * Ctor   
         */

        public FoldoutHandler(string dataKey = null, bool indent = true)
        {
            _dataKey = dataKey;
            _indent = indent;
            
            var data = EditorPrefs.GetString(_dataKey);
            
            if (string.IsNullOrWhiteSpace(data))
            {
                _data = new Dictionary<string, bool>();
                return;
            }

            var dictionary = new Dictionary<string, bool>();
            var lines = data.Split('$');
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                    
                var entries = line.Split('§');
                var key = entries[0];
                var value = bool.Parse(entries[1]);
                    
                dictionary.Add(key, value);
            }

            _data = dictionary;

        }

        public void SaveState()
        {
            var data = string.Empty;
            foreach (var (key, value) in Data)
            {
                data += $"${key}§{value}";
            }
            EditorPrefs.SetString(_dataKey, data);
        }

        public void ForceHeader(string name)
        {
            Foldout(true, name);
        }

        public bool this[string name]
        {
            get
            {
                if (!_data.TryGetValue(name, out var currentValue))
                {
                    _data.Add(name, currentValue = !DefaultFoldoutStates.TryGetValue(name, out var state) || state);
                }
                    
                var newValue = _indent ? Foldout(currentValue, name): WindowFoldout(currentValue, name);
                this[name] = newValue;

                if (newValue != currentValue && Event.current.alt)
                {
                    if (newValue)
                    {
                        var keys = Data.Keys.Select(key => key).ToArray();
                        foreach (var key in keys)
                        {
                            Data[key] = true;
                        }
                    }
                    else
                    {
                        var keys = Data.Keys.Select(key => key).ToArray();
                        foreach (var key in keys)
                        {
                            Data[key] = false;
                        }
                    }
                }
                    
                return currentValue;
            }
            private set
            {
                if (!_data.TryAdd(name, value))
                {
                    _data[name] = value;
                }
            }
        }

        #region --- GUI ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Foldout(bool value, string label)
        {
            EditorGUILayout.LabelField("");
            var lastRect = GUILayoutUtility.GetLastRect();
            var widthRect = new Rect(0, lastRect.y, EditorGUIUtility.currentViewWidth, lastRect.height + 2);
            var foldoutRect = new Rect(20, lastRect.y +1, EditorGUIUtility.currentViewWidth - 10, lastRect.height);
            EditorGUI.DrawRect(new Rect(0, lastRect.y, EditorGUIUtility.currentViewWidth, 1), new Color(0f, 0f, 0f, 0.3f));
            EditorGUI.DrawRect(widthRect, new Color(0f, 0f, 0f, 0.15f));
            return EditorGUI.Foldout(foldoutRect,value, label, true);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WindowFoldout(bool value, string label)
        {
            EditorGUILayout.LabelField("");
            var lastRect = GUILayoutUtility.GetLastRect();
            var widthRect = new Rect(0, lastRect.y, EditorGUIUtility.currentViewWidth, lastRect.height + 2);
            var foldoutRect = new Rect(0, lastRect.y +1, EditorGUIUtility.currentViewWidth - 10, lastRect.height);
            EditorGUI.DrawRect(new Rect(0, lastRect.y, EditorGUIUtility.currentViewWidth, 1), new Color(0f, 0f, 0f, 0.3f));
            EditorGUI.DrawRect(widthRect, new Color(0f, 0f, 0f, 0.15f));
            return EditorGUI.Foldout(foldoutRect,value, label, true);
        }
        
        
        #endregion
    }
}