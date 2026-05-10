using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveEntry
{
    public EnemyDataSO enemyData;
    public int count;
    public float spawnInterval = 0.5f;  // seconds between each spawn in this entry
    public string spawnGroupTag;         // which SpawnPoints to use (empty = any)
}

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Game/Wave Config")]
public class WaveDataSO : ScriptableObject
{
    public string waveName;
    public List<WaveEntry> entries = new();
    public float delayBeforeWave = 2f;
    public bool isBossWave = false;
}