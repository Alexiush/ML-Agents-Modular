using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using UnityEngine;

public static class SaveUtilities
{
    public static void EnsureFolderExists(string path)
    {
        var directoriesHierarchy = path.Split('\\');
        var currentPath = "";

        foreach (var directory in directoriesHierarchy)
        {
            if (AssetDatabase.IsValidFolder($"{currentPath}\\{directory}"))
            {
                continue;
            }

            AssetDatabase.CreateFolder(currentPath, directory);
            currentPath = System.IO.Path.Combine(currentPath, directory);
        }
    }

    public static T GetAsset<T>(string path, string assetName) where T : ScriptableObject
    {
        string fullPath = $"{path}\\{assetName}.asset";

        T asset = AssetDatabase.LoadAssetAtPath<T>(fullPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, fullPath);
        }
        
        return asset;
    }

    public static void SaveAssetsImmediately()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
