using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private SpawnPoint[] allSpawnPoints;

    // State
    private LevelDataSO levelData;
    private int waveIndex = 0;
    private int activeEnemyCount = 0;
    private bool isSpawning = false;
    private bool levelFinished = false;

    private readonly Dictionary<GameObject, GameObject> activePrefabMap = new();

    public event System.Action<int, int> OnWaveChanged;
    public event System.Action OnLevelComplete;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Entry point called by LevelManager ────────────────────

    public void LoadLevel(LevelDataSO data)
    {
        levelData = data;
        waveIndex = 0;
        activeEnemyCount = 0;
        isSpawning = false;
        levelFinished = false;
        activePrefabMap.Clear();

        PrewarmAllPools();
        StartCoroutine(RunWave(waveIndex));
    }

    // ── Pool prewarm ───────────────────────────────────────────

    private void PrewarmAllPools()
    {
        if (levelData == null || levelData.waves == null)
        {
            Debug.LogError("[EnemyManager] levelData or waves is null!");
            return;
        }

        if (EnemyPool.Instance == null)
        {
            Debug.LogError("[EnemyManager] EnemyPool.Instance is null! Check Script Execution Order.");
            return;
        }

        foreach (WaveDataSO wave in levelData.waves)
        {
            if (wave == null || wave.entries == null) continue;

            foreach (WaveEntry entry in wave.entries)
            {
                if (entry == null || entry.enemyData == null)
                {
                    Debug.LogWarning("[EnemyManager] Null entry or enemyData found in wave!");
                    continue;
                }

                if (entry.enemyData.prefab == null)
                {
                    Debug.LogWarning($"[EnemyManager] Prefab missing on: {entry.enemyData.name}");
                    continue;
                }

                EnemyPool.Instance.Prewarm(entry.enemyData.prefab, entry.enemyData.prewarmCount);
            }
        }
    }
    // ── Wave runner ────────────────────────────────────────────

    private IEnumerator RunWave(int index)
    {
        if (index >= levelData.waves.Count)
        {
            CompletLevel();
            yield break;
        }

        WaveDataSO wave = levelData.waves[index];
        OnWaveChanged?.Invoke(index + 1, levelData.waves.Count);

        Debug.Log($"[EnemyManager] Wave {index + 1}/{levelData.waves.Count} starting...");

        yield return new WaitForSeconds(wave.delayBeforeWave);

        // Mark spawning BEFORE spawning any enemy
        isSpawning = true;
        activeEnemyCount = 0;

        foreach (WaveEntry entry in wave.entries)
            yield return SpawnEntry(entry);

        // All enemies for this wave are now spawned
        isSpawning = false;

        Debug.Log($"[EnemyManager] Wave {index + 1} fully spawned. " +
                  $"Active enemies: {activeEnemyCount}");

        // Edge case: wave had 0 entries — advance immediately
        if (activeEnemyCount == 0)
            AdvanceWave();
    }

    private IEnumerator SpawnEntry(WaveEntry entry)
    {
        Debug.Log($"[EnemyManager] SpawnEntry: count={entry.count}, enemyData={entry.enemyData?.name ?? "NULL"}, prefab={entry.enemyData?.prefab?.name ?? "NULL"}");

        for (int i = 0; i < entry.count; i++)
        {
            Debug.Log($"[EnemyManager] Spawning enemy {i + 1}/{entry.count}");
            SpawnEnemy(entry);
            yield return new WaitForSeconds(entry.spawnInterval);
        }
    }

    private void SpawnEnemy(WaveEntry entry)
    {
        SpawnPoint point = GetSpawnPoint(entry.spawnGroupTag);
        Debug.Log($"[EnemyManager] SpawnPoint: {(point == null ? "NULL" : point.name)}");
        if (point == null) return;

        GameObject obj = EnemyPool.Instance.Get(entry.enemyData.prefab, point.transform.position, point.transform.rotation);
        Debug.Log($"[EnemyManager] Pooled obj: {(obj == null ? "NULL" : obj.name)}");
        if (obj == null) return;

        activeEnemyCount++;
        activePrefabMap[obj] = entry.enemyData.prefab;

        EnemyAI ai = obj.GetComponent<EnemyAI>();
        Debug.Log($"[EnemyManager] EnemyAI: {(ai == null ? "NULL" : "found")}");

        List<Transform> waypoints = GetWaypointsNear(point);
        ai.Initialize(entry.enemyData, player, waypoints, entry.enemyData.prefab);
    }

    // ── Spawn point helpers ────────────────────────────────────

    private SpawnPoint GetSpawnPoint(string groupTag)
    {
        var candidates = new List<SpawnPoint>();
        foreach (SpawnPoint sp in allSpawnPoints)
            if (string.IsNullOrEmpty(groupTag) || sp.groupTag == groupTag)
                candidates.Add(sp);

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    private List<Transform> GetWaypointsNear(SpawnPoint point)
    {
        var waypoints = new List<Transform>();
        Transform group = point.transform.parent;
        if (group == null) return waypoints;

        foreach (Transform child in group)
        {
            if (child == point.transform) continue;
            if (child.CompareTag("Waypoint"))
                waypoints.Add(child);
        }

        return waypoints;
    }

    // ── Enemy death ────────────────────────────────────────────

    public void OnEnemyDied(GameObject enemy, GameObject sourcePrefab)
    {
        StartCoroutine(ReturnToPoolNextFrame(enemy, sourcePrefab));
    }

    private IEnumerator ReturnToPoolNextFrame(GameObject enemy, GameObject sourcePrefab)
    {
        yield return null;

        // Guard: already returned to pool
        if (!activePrefabMap.ContainsKey(enemy))
        {
            Debug.LogWarning($"[EnemyManager] Tried to return enemy that isn't tracked: {enemy.name}");
            yield break;
        }

        EnemyPool.Instance.Return(sourcePrefab, enemy);
        activePrefabMap.Remove(enemy);
        activeEnemyCount--;

        Debug.Log($"[EnemyManager] Enemy died. Remaining: {activeEnemyCount} | " +
                  $"Still spawning: {isSpawning}");

        if (activeEnemyCount <= 0 && !isSpawning)
            AdvanceWave();
    }

    // ── Wave / level progression ───────────────────────────────

    private void AdvanceWave()
    {
        if (levelFinished) return;

        waveIndex++;

        if (waveIndex >= levelData.waves.Count)
        {
            CompletLevel();
            return;
        }

        Debug.Log($"[EnemyManager] Advancing to wave {waveIndex + 1}");
        StartCoroutine(RunWave(waveIndex));
    }

    private void CompletLevel()
    {
        if (levelFinished) return;
        levelFinished = true;

        Debug.Log("[EnemyManager] All waves complete. Level done.");
        OnLevelComplete?.Invoke();
    }
}