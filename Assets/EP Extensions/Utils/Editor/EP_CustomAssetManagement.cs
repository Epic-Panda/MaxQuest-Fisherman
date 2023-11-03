using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EP.Utils.Editor
{
    public static class EP_CustomAssetManagement
    {
        public static IEnumerable<T> FindAssetsOfType<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
        }

        public static T CreateScriptableObjectAsset<T>(string directory, string assetName, bool selectAsset = true) where T : ScriptableObject
        {
            System.IO.Directory.CreateDirectory(directory);

            T newAsset = AssetDatabase.LoadAssetAtPath<T>(directory + assetName);

            if(newAsset == null)
            {
                newAsset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(newAsset, directory + assetName);
                AssetDatabase.SaveAssets();
            }

            if(selectAsset)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newAsset;
            }

            return newAsset;
        }

        public static T FindOrCreateBuiltInObjectAsset<T>(string directory, string assetName, bool selectAsset = true) where T : Object
        {
            System.IO.Directory.CreateDirectory(directory);

            T newAsset = AssetDatabase.LoadAssetAtPath<T>(directory + assetName);

            if(newAsset == null)
            {
                newAsset = System.Activator.CreateInstance<T>();
                AssetDatabase.CreateAsset(newAsset, directory + assetName);
                AssetDatabase.SaveAssets();
            }

            if(selectAsset)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newAsset;
            }

            return newAsset;
        }
    }
}
