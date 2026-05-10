using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image delayedFillImage;  // ghost bar (lags behind)
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float delayedFillSpeed = 2f;
    [SerializeField] private float fadeOutDelay = 3f;    // 0 = never fade
    [SerializeField] private bool autoFadeWhenFull = true;

    [Header("Colors")]
    [SerializeField] private Color highHealthColor = new Color(0.18f, 0.8f, 0.28f);
    [SerializeField] private Color midHealthColor  = new Color(0.95f, 0.75f, 0.1f);
    [SerializeField] private Color lowHealthColor  = new Color(0.85f, 0.15f, 0.15f);

    private float targetFill;
    private Coroutine fadeCoroutine;
    private Coroutine delayedFillCoroutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetHealth(float current, float max)
    {
        targetFill = Mathf.Clamp01(current / max);

        // Snap fill image immediately
        fillImage.fillAmount = targetFill;
        fillImage.color = EvaluateColor(targetFill);

        // Delayed ghost bar
        if (delayedFillImage != null)
        {
            if (delayedFillCoroutine != null)
                StopCoroutine(delayedFillCoroutine);
            delayedFillCoroutine = StartCoroutine(AnimateDelayedFill());
        }

        // Cancel any ongoing fade and show
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        canvasGroup.alpha = 1f;

        // Auto-fade when full
        if (autoFadeWhenFull && targetFill >= 1f)
            fadeCoroutine = StartCoroutine(FadeOut(fadeOutDelay));
        else if (fadeOutDelay > 0f)
            fadeCoroutine = StartCoroutine(FadeOut(fadeOutDelay));
    }

    public void ForceShow()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        canvasGroup.alpha = 1f;
    }

    public void ForceHide()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        canvasGroup.alpha = 0f;
    }

    private IEnumerator AnimateDelayedFill()
    {
        yield return new WaitForSeconds(0.4f); // pause before ghost drains

        while (Mathf.Abs(delayedFillImage.fillAmount - targetFill) > 0.001f)
        {
            delayedFillImage.fillAmount = Mathf.MoveTowards(
                delayedFillImage.fillAmount,
                targetFill,
                delayedFillSpeed * Time.deltaTime
            );
            yield return null;
        }

        delayedFillImage.fillAmount = targetFill;
    }

    private IEnumerator FadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * 2f;
            yield return null;
        }
    }

    private Color EvaluateColor(float t)
    {
        if (t > 0.5f)
            return Color.Lerp(midHealthColor, highHealthColor, (t - 0.5f) * 2f);
        return Color.Lerp(lowHealthColor, midHealthColor, t * 2f);
    }
}