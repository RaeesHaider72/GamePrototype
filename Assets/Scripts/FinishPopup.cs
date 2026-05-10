using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class FinishPopup : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text headingTxt;
    public TMP_Text descriptionTxt;
    public TMP_Text nextLevelBtnTxt;

    [Header("UI")]
    public Button nextLevelBtn;
    public GameObject winImage;
    public GameObject loseImage;
    public CanvasGroup canvasGroup;
    public RectTransform popupPanel;

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.3f;

    private void Awake()
    {
        // Start hidden
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (popupPanel != null)
            popupPanel.localScale = Vector3.zero;
    }

    private void OnEnable()
    {
        nextLevelBtn.onClick.RemoveAllListeners();

        switch (GameManager.Instance.gameStatus)
        {
            case GameStatus.win:
                SetupWin();
                break;

            case GameStatus.lose:
                SetupLose();
                break;

            case GameStatus.complete:
                SetupComplete();
                break;
        }

        AnimateIn();
    }

    // ── Setup per state ────────────────────────────────────────

    private void SetupWin()
    {
        winImage.SetActive(true);
        loseImage.SetActive(false);

        int completedLevel = LevelManager.Instance.currentLevelIndex; // already incremented
        int totalLevels = LevelManager.Instance.TotalLevels;

        headingTxt.text = "Level Complete!";
        descriptionTxt.text = $"Level {completedLevel} / {totalLevels} done.\nGet ready for the next one!";
        nextLevelBtnTxt.text = "Next Level";

        nextLevelBtn.onClick.AddListener(OnNextLevel);
    }

    private void SetupLose()
    {
        winImage.SetActive(false);
        loseImage.SetActive(true);

        headingTxt.text = "You Died!";
        descriptionTxt.text = "Don't give up. Try again!";
        nextLevelBtnTxt.text = "Retry";

        nextLevelBtn.onClick.AddListener(OnRetry);
    }

    private void SetupComplete()
    {
        winImage.SetActive(true);
        loseImage.SetActive(false);

        headingTxt.text = "You Win!";
        descriptionTxt.text = "All levels conquered!\nYou are a legend.";
        nextLevelBtnTxt.text = "Main Menu";

        nextLevelBtn.onClick.AddListener(OnMainMenu);
    }

    // ── Button handlers ────────────────────────────────────────

    private void OnNextLevel()
    {
        AnimateOut(() =>
        {
            LevelManager.Instance.LoadLevel(LevelManager.Instance.currentLevelIndex);
            gameObject.SetActive(false);
        });
    }

    private void OnRetry()
    {
        AnimateOut(() =>
        {
            LevelManager.Instance.RestartCurrentLevel();
            gameObject.SetActive(false);
        });
    }

    private void OnMainMenu()
    {
        AnimateOut(() => SceneManager.LoadScene("Splash"));
    }

    // ── Public API ─────────────────────────────────────────────

    public void Setup(string heading, string description)
    {
        headingTxt.text = heading;
        descriptionTxt.text = description;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        // Start() will handle setup and animation
    }

    // ── Animation ──────────────────────────────────────────────

    private void AnimateIn()
    {
        if (canvasGroup == null || popupPanel == null) return;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        popupPanel.localScale = Vector3.one * 0.8f;

        canvasGroup.DOFade(1f, fadeInDuration);
        popupPanel.DOScale(1f, fadeInDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
                popupPanel.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.5f));
    }

    private void AnimateOut(System.Action onComplete)
    {
        if (canvasGroup == null || popupPanel == null)
        {
            onComplete?.Invoke();
            return;
        }

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        canvasGroup.DOFade(0f, 0.2f);
        popupPanel.DOScale(0.8f, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => onComplete?.Invoke());
    }
}