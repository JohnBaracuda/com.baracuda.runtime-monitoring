// Copyright (c) 2022 Jonathan Lang
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    public class FoldoutHandler
    {
        /*
         * Properties    
         */
        private Dictionary<string, bool> Data { get; }

        public Dictionary<string, bool> DefaultFoldoutStates { get; } = new Dictionary<string, bool>();

        /*
         * Fields   
         */

        private readonly string _dataKey;

        /*
         * Ctor   
         */

        public FoldoutHandler(string dataKey = null)
        {
            _dataKey = dataKey;
            
            var data = EditorPrefs.GetString(_dataKey);
            
            if (string.IsNullOrWhiteSpace(data))
            {
                Data = new Dictionary<string, bool>();
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

            Data = dictionary;

        }

        public void SaveState()
        {
            var data = string.Empty;
            foreach (var entry in Data)
            {
                data += $"${entry.Key}§{entry.Value}";
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
                if (!Data.TryGetValue(name, out var currentValue))
                {
                    Data.Add(name, currentValue = !DefaultFoldoutStates.TryGetValue(name, out var state) || state);
                }
                    
                var newValue = Foldout(currentValue, name);
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
                if (Data.ContainsKey(name))
                {
                    Data[name] = value;
                }
                else
                {
                    Data.Add(name, value);
                }
            }
        }

        #region --- GUI ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Foldout(bool value, string label)
        {
            var lastRect = EditorGUILayout.GetControlRect();
            var widthRect = new Rect(0, lastRect.y, EditorGUIUtility.currentViewWidth, lastRect.height + 2);
            EditorGUI.DrawRect(new Rect(0, lastRect.y, EditorGUIUtility.currentViewWidth, 1), new Color(0f, 0f, 0f, 0.3f));
            EditorGUI.DrawRect(widthRect, new Color(0f, 0f, 0f, 0.15f));
            return EditorGUI.Foldout(lastRect,value, label, true);
        }
       
        
        #endregion
    }
}