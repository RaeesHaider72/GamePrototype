using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #endregion

    public GameStatus gameStatus;

    [Header("References")]
    public FinishPopup finishPopup;
    public Camera healthBarCamera;

    // ── Player events ──────────────────────────────────────────

    public void OnPlayerDeath()
    {
        gameStatus = GameStatus.lose;
        ShowPopup("You Died!", "Don't give up. Try again!");
    }

    public void OnPlayerWin()
    {
        gameStatus = GameStatus.win;

        int completedLevel = LevelManager.Instance.currentLevelIndex;
        int totalLevels = LevelManager.Instance.TotalLevels;

        ShowPopup(
            "Level Complete!",
            $"Level {completedLevel} / {totalLevels} done.\nGet ready for the next one!"
        );
    }

    public void AllLevelsComplete()
    {
        gameStatus = GameStatus.complete;
        ShowPopup(
            "You Win!",
            "All levels conquered!\nYou are a legend."
        );
    }

    // ── Internal ───────────────────────────────────────────────

    private void ShowPopup(string heading, string description)
    {
        if (finishPopup == null)
        {
            Debug.LogError("[GameManager] FinishPopup reference is missing!");
            return;
        }

        finishPopup.Setup(heading, description);
        finishPopup.gameObject.SetActive(true);
    }
}

public enum GameStatus { win, lose, complete }