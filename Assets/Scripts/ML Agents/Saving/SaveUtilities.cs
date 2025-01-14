using UnityEditor;

namespace ModularMLAgents.Saving
{
    public static class SaveUtilities
    {
        public static void EnsureFolderExists(string path)
        {
            var directoriesHierarchy = path.Split('\\');
            var currentPath = "Assets";

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

        public static void SaveAssetsImmediately()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
