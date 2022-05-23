using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
            /// <summary>
        /// Method will change region spelling form --- [REGION] --- TO --- Region ---
        /// Add a [MenuItem("Tools/FixRegions")] Attribute to use
        /// </summary>
        [MenuItem("Tools/FixRegions")]
        private static void FixRegions()
        {
            var paths = AssetDatabase.GetAllAssetPaths();

            var toCheck = "// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)";
            var replacement = "// Copyright (c) 2022 Jonathan Lang" + Environment.NewLine;
            
            foreach(string assetPath in paths)
            {
                if (!assetPath.StartsWith("Assets/Baracuda"))
                {
                    continue;
                }
            
                if(assetPath.EndsWith(".cs"))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                    
                    var text = asset.text;
                    if (text.StartsWith(toCheck))
                    {
                        File.WriteAllText(assetPath, text.Replace(toCheck, replacement));
                        EditorUtility.SetDirty(asset);
                    }
                }
            }
        }
}
