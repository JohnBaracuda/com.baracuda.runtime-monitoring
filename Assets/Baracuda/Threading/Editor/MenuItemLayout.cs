// Copyright (c) 2022 Jonathan Lang
using UnityEngine;

namespace Baracuda.Threading.Editor
{
    internal static class MenuItemLayout
    {
        [UnityEditor.MenuItem("Tools/Dispatcher/Validate Scene Component", priority = 2361)]
        private static void ValidateSceneComponent()
        {
            UnityEditor.Selection.activeGameObject = Dispatcher.Current.gameObject;
        }
        
        [UnityEditor.MenuItem("Tools/Dispatcher/Script Execution Order", priority = 2360)]
        private static void OpenSettings()
        {
            ExecutionOrderWindow.Open();
        }
        
        [UnityEditor.MenuItem("Tools/Dispatcher/Documentation", priority = 2365)]
        private static void OpenDocumentationComponent()
        {
            Application.OpenURL("https://johnbaracuda.com/dispatcher.html");
        }
    }
}