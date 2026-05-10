using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : HealthComponent
{
    [Header("Player Specific")]
    [SerializeField] private int lives = 3;
    [SerializeField] private float shieldHealth = 50f;
    [SerializeField] private float invincibilityDuration = 1.2f;

    [Header("Player Events")]
    public UnityEvent<int> OnLivesChanged;
    public UnityEvent OnShieldBroken;
    public UnityEvent OnRespawned;

    private float currentShield;
    private float invincibilityTimer;
    private Rigidbody rb;

    public int Lives => lives;
    public float ShieldPercent => shieldHealth > 0 ? currentShield / shieldHealth : 0f;

    protected override void Awake()
    {
        base.Awake();
        currentShield = shieldHealth;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
                IsInvincible = false;
        }
    }

    public override void TakeDamage(DamageInfo info)
    {
        if (currentShield > 0f)
        {
            float overflow = AbsorbWithShield(info.amount);
            if (overflow <= 0f) return;

            DamageInfo overflowInfo = info;
            overflowInfo.amount = overflow;
            base.TakeDamage(overflowInfo);
        }
        else
        {
            base.TakeDamage(info);
        }

        if (rb != null && info.knockbackForce > 0f)
            rb.AddForce(info.knockbackDir * info.knockbackForce, ForceMode.Impulse);

        TriggerInvincibility(invincibilityDuration);
    }

    private float AbsorbWithShield(float damage)
    {
        if (currentShield >= damage)
        {
            currentShield -= damage;
            if (currentShield <= 0f) OnShieldBroken?.Invoke();
            return 0f;
        }

        float overflow = damage - currentShield;
        currentShield = 0f;
        OnShieldBroken?.Invoke();
        return overflow;
    }

    public void RegenerateShield(float amount)
    {
        currentShield = Mathf.Clamp(currentShield + amount, 0f, shieldHealth);
    }

    public void TriggerInvincibility(float duration)
    {
        IsInvincible = true;
        invincibilityTimer = duration;
    }

    protected override void OnDeath()
    {
        lives--;
        OnLivesChanged?.Invoke(lives);

        if (lives > 0)
        {
            Respawn();
        }
        else
        {
            GameManager.Instance.OnPlayerDeath();  
        }
    }

   private void Respawn()
    {
        currentHealth = maxHealth;
        currentShield = shieldHealth;
        TriggerInvincibility(2f);
        OnRespawned?.Invoke();
    }
}