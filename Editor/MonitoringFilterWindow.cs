// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Baracuda.Monitoring.Editor
{
    public class MonitoringFilterWindow : EditorWindow
    {
        [SerializeField]
        private List<string> filterQueue = new List<string>(25);
        private string _lastFilter;
        private Vector2 _scrollPosition;
        private bool _isEnabled;

        private bool HasChanged => Filter != _lastFilter;

        private string Filter { get; set; }

        public static void Open()
        {
            GetWindow<MonitoringFilterWindow>("Runtime Monitoring Filter").Show();
        }

        private void OnEnable()
        {
            Filter = EditorPrefs.GetString($"{nameof(MonitoringFilterWindow)}{nameof(Filter)}");
            CacheFilter();
        }

        private void OnDisable()
        {
            EditorPrefs.SetString($"{nameof(MonitoringFilterWindow)}{nameof(Filter)}", Filter);
        }

        private void OnGUI()
        {
            _isEnabled = Application.isPlaying && Monitor.Initialized;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("UI filtering is only available in play mode!", MessageType.Info);
            }
            else if (!Monitor.Initialized)
            {
                EditorGUILayout.HelpBox("Waiting for Monitoring Systems to be initialized!", MessageType.Info);
            }

            DrawFilterField();
            DrawButtons();
            DrawInfo();

            if (_isEnabled && HasChanged)
            {
                if (!string.IsNullOrWhiteSpace(Filter))
                {
                    Monitor.UI.ApplyFilter(Filter);
                    _lastFilter = Filter;
                }
                else
                {
                    Monitor.UI.ResetFilter();
                }
            }
        }

        private void DrawFilterField()
        {
            GUILayout.BeginHorizontal("helpBox");
            Filter = GUILayout.TextField(Filter, new GUIStyle("ToolbarSeachTextField") {fixedHeight = 20f});

            if (GUILayout.Button("Reset", GUILayout.Width(100)))
            {
                Filter = string.Empty;
            }
            GUILayout.EndHorizontal();

            TryCacheFilter();
        }

        private void DrawButtons()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            var tags = Monitor.Registry.UsedTags;
            var types = Monitor.Registry.UsedTypeNames;

            GUILayout.BeginHorizontal();
            DrawCollection(tags, "Tags", 3);
            DrawCollection(types, "Types", 3);
            DrawCollection(filterQueue, "History", 3);
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        private void DrawCollection(IReadOnlyList<string> collection, string displayName, int rows)
        {
            var titleStyle = new GUIStyle("ToolbarButton")
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(-2, -2, 5, 5),
                fixedWidth = position.width / rows - 10,
                fontStyle = FontStyle.Bold,
                fontSize = 12
            };

            var btnStyle = new GUIStyle("Button")
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(-rows, -rows, 0, 0),
                fixedWidth = position.width / rows - 10
            };

            GUILayout.BeginVertical();
            GUILayout.Label(displayName, titleStyle);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            for (var i = collection.Count - 1; i >= 0; i--)
            {
                var item = collection[i];
                if (GUILayout.Button(item, btnStyle))
                {
                    if (Event.current.shift)
                    {
                        AddToFilter(item);
                        continue;
                    }
                    if (Event.current.control)
                    {
                        RemoveFromFilter(item);
                        continue;
                    }

                    Filter = item;
                    CacheFilter();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void AddToFilter(string item)
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                Filter = item;
                return;
            }

            var activeFilter = Filter.Split(Monitor.Settings.FilterAppendSymbol);
            for (var i = 0; i < activeFilter.Length; i++)
            {
                var activeSymbol = activeFilter[i].Replace(" ", "");
                if (activeSymbol.Equals(item))
                {
                    return;
                }
            }

            Filter = $"{Filter} {Monitor.Settings.FilterAppendSymbol.ToString()} {item}";
            CacheFilter();
        }

        private void RemoveFromFilter(string item)
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                return;
            }

            var activeFilter = Filter.Split(Monitor.Settings.FilterAppendSymbol);
            var sb = new StringBuilder();
            for (var i = 0; i < activeFilter.Length; i++)
            {
                var activeSymbol = activeFilter[i].Replace(" ", "");
                if (activeSymbol.Equals(item))
                {
                    continue;
                }

                sb.Append(activeSymbol);
                sb.Append(' ');
                sb.Append(Monitor.Settings.FilterAppendSymbol);
                sb.Append(' ');
            }

            if (sb.Length > 3)
            {
                sb.Remove(sb.Length - 3, 3);
            }

            Filter = sb.ToString();
            CacheFilter();
        }

        private void TryCacheFilter()
        {
            if (Event.current.keyCode == KeyCode.Return)
            {
                CacheFilter();
                return;
            }

            if (Event.current.keyCode == KeyCode.KeypadEnter)
            {
                CacheFilter();
                return;
            }

            if (Filter.EndsWith(" ") && !Filter.EndsWith($" {Monitor.Settings.FilterAppendSymbol} "))
            {
                CacheFilter();
            }
        }

        private void CacheFilter()
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                return;
            }

            if (filterQueue.Contains(Filter))
            {
                filterQueue.Remove(Filter);
            }

            filterQueue.Add(Filter);

            if (filterQueue.Count > 24)
            {
                filterQueue.RemoveAt(0);
            }
        }

        private void DrawInfo()
        {
            GUILayout.BeginVertical("helpBox");
            EditorGUILayout.LabelField($"Hold shift when clicking on tags etc. to combine multiple filter.");
            EditorGUILayout.LabelField($"Hold shift when clicking on tags etc. to remove certain filter.");
            EditorGUILayout.LabelField($"Use {Monitor.Settings.FilterAppendSymbol.ToString()} to combine multiple search queries. (Hold shift when clicking on tags etc.)");
            EditorGUILayout.LabelField($"Use {Monitor.Settings.FilterTagsSymbol.ToString()} as a prefix to only filter for tags.");
            EditorGUILayout.LabelField($"Use {Monitor.Settings.FilterAbsoluteSymbol.ToString()} as a prefix to only filter for exact matches.");
            EditorGUILayout.LabelField($"Use {Monitor.Settings.FilterNegateSymbol.ToString()} as a prefix to exclude matching results.");

            GUILayout.EndVertical();
        }
    }
}
