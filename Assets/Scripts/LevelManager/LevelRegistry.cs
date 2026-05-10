using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelRegistry", menuName = "Game/Level Registry")]
public class LevelRegistry : ScriptableObject
{
    [System.Serializable]
    public class LevelEntry
    {
        public string sceneName;        // must match Build Settings scene name exactly
        public LevelDataSO levelData;
        public string displayName;      // "Level 1 - The Jungle"
        public Sprite thumbnail;        // for level select screen
        public bool isLocked = true;
    }

    public List<LevelEntry> levels = new();

    // Get level data by scene name
    public LevelEntry GetByScene(string sceneName)
    {
        return levels.Find(l => l.sceneName == sceneName);
    }

    // Get level data by index
    public LevelEntry GetByIndex(int index)
    {
        if (index < 0 || index >= levels.Count) return null;
        return levels[index];
    }

    public int TotalLevels => levels.Count;
}