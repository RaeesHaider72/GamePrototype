#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts In All Assets")]
    public static void FindAll()
    {
        int count = 0;

        // Search all prefabs and assets
        string[] allAssets = AssetDatabase.GetAllAssetPaths();

        foreach (string path in allAssets)
        {
            // Only check prefabs
            if (!path.EndsWith(".prefab")) continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            foreach (Component c in components)
            {
                if (c == null)
                {
                    Debug.LogWarning($"Missing script in PREFAB: {path}", prefab);
                    count++;
                }
            }
        }

        Debug.Log($"Found {count} missing scripts in assets.");
    }
}
#endif