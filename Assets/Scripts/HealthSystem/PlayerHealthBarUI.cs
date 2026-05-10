using UnityEngine;
using TMPro;

public class PlayerHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthBarUI healthBarUI;
    [SerializeField] private TextMeshProUGUI healthText;   // "85 / 100"
    [SerializeField] private TextMeshProUGUI livesText;    // "x 3"
    [SerializeField] private TextMeshProUGUI shieldText;   // shield amount

    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("[PlayerHealthBarUI] No PlayerHealth found in scene.");
            return;
        }

        SubscribeToEvents();
        RefreshAll();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        playerHealth.events.OnHealthChanged.AddListener(OnHealthChanged);
        playerHealth.OnLivesChanged.AddListener(OnLivesChanged);
        playerHealth.OnShieldBroken.AddListener(OnShieldBroken);
    }

    private void UnsubscribeFromEvents()
    {
        if (playerHealth == null) return;
        playerHealth.events.OnHealthChanged.RemoveListener(OnHealthChanged);
        playerHealth.OnLivesChanged.RemoveListener(OnLivesChanged);
        playerHealth.OnShieldBroken.RemoveListener(OnShieldBroken);
    }

    private void OnHealthChanged(float current, float max)
    {
        healthBarUI.SetHealth(current, max);

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void OnLivesChanged(int lives)
    {
        if (livesText != null)
            livesText.text = $"x {lives}";
    }

    private void OnShieldBroken()
    {
        if (shieldText != null)
            shieldText.text = "SHIELD BROKEN";
    }

    private void RefreshAll()
    {
        OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        OnLivesChanged(playerHealth.Lives);
    }
}