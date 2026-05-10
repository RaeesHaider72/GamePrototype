using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Registry")]
    [SerializeField] private LevelRegistry levelRegistry;

    [Header("References")]
    [SerializeField] private EnemyManager enemyManager;

    public TMP_Text levelNumber;
    public int currentLevelIndex = 0;

    // Track total waves across all levels for popup display
    private int totalWavesSurvived = 0;

    public int TotalLevels => levelRegistry.TotalLevels;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        enemyManager.OnLevelComplete += OnLevelComplete;
        enemyManager.OnWaveChanged += OnWaveChanged;
        LoadLevel(currentLevelIndex);
    }

    private void OnDestroy()
    {
        if (enemyManager == null) return;
        enemyManager.OnLevelComplete -= OnLevelComplete;
        enemyManager.OnWaveChanged -= OnWaveChanged;
    }

    public void LoadLevel(int index)
    {
        if (index >= levelRegistry.TotalLevels)
        {
            Debug.Log("[LevelManager] All levels complete!");
            
            return;
        }

        LevelRegistry.LevelEntry entry = levelRegistry.GetByIndex(index);
        if (entry == null) return;

        Debug.Log($"[LevelManager] Loading level {index + 1}: {entry.displayName}");

        if (entry.levelData.skybox != null)
            RenderSettings.skybox = entry.levelData.skybox;

        if (levelNumber != null)
            levelNumber.text = $"Level # {index + 1}";

        enemyManager.LoadLevel(entry.levelData);
    }

    private void OnWaveChanged(int current, int total)
    {
        // Count each wave survived for popup display
        if (current > 1) totalWavesSurvived++;
    }

    private void OnLevelComplete()
    {
        totalWavesSurvived++; // count the final wave too

        int completedLevel = currentLevelIndex + 1;
        int totalLevels = levelRegistry.TotalLevels;
        bool isLastLevel = currentLevelIndex + 1 >= totalLevels;

        currentLevelIndex++;

        if (isLastLevel)
        {
            
            GameManager.Instance.AllLevelsComplete();
        }
        else
        {
            
            GameManager.Instance.OnPlayerWin();
        }

        Debug.Log($"[LevelManager] Level {completedLevel} complete!");
    }

    public void RestartCurrentLevel()
    {
        totalWavesSurvived = 0;
        LoadLevel(currentLevelIndex);
    }
}