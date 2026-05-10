using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HealthEvents
{
    public UnityEvent<DamageInfo> OnDamaged;
    public UnityEvent<float> OnHealed;          // passes heal amount
    public UnityEvent<DamageInfo> OnDeath;
    public UnityEvent<float, float> OnHealthChanged; // current, max
}

public abstract class HealthComponent : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float armor = 0f;        // flat damage reduction
    [SerializeField] [Range(0f, 0.9f)] protected float armorPercent = 0f; // % reduction

    [Header("State")]
    [SerializeField] private bool isInvincible = false;

    [Header("Events")]
    public HealthEvents events;

    protected float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0f;
    public float HealthPercent => currentHealth / maxHealth;
    public bool IsInvincible { get => isInvincible; set => isInvincible = value; }

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(DamageInfo info)
    {
        if (!IsAlive || isInvincible) return;

        float mitigated = CalculateMitigation(info);
        currentHealth = Mathf.Clamp(currentHealth - mitigated, 0f, maxHealth);

        // Build a new info with the actual damage dealt
        DamageInfo actualInfo = info;
        actualInfo.amount = mitigated;

        events.OnDamaged?.Invoke(actualInfo);
        events.OnHealthChanged?.Invoke(currentHealth, maxHealth);

        HealthEventBus.Publish(new HealthEvent
        {
            target = gameObject,
            info = actualInfo,
            type = HealthEventType.Damaged,
            remainingHealth = currentHealth
        });

        OnAfterDamage(actualInfo);

        if (currentHealth <= 0f) Die();
    }

    public virtual void Heal(float amount)
    {
        if (!IsAlive) return;

        float healed = Mathf.Min(amount, maxHealth - currentHealth);
        currentHealth += healed;

        events.OnHealed?.Invoke(healed);
        events.OnHealthChanged?.Invoke(currentHealth, maxHealth);

        HealthEventBus.Publish(new HealthEvent
        {
            target = gameObject,
            type = HealthEventType.Healed,
            remainingHealth = currentHealth
        });
    }

    // In HealthComponent
private bool isDead = false;

public virtual void Die()
{
    if (isDead) return; // prevent double death
    isDead = true;
    
    currentHealth = 0f;
    events.OnDeath?.Invoke(new DamageInfo());
    events.OnHealthChanged?.Invoke(0f, maxHealth);

    HealthEventBus.Publish(new HealthEvent
    {
        target = gameObject,
        type = HealthEventType.Died,
        remainingHealth = 0f
    });

    OnDeath();
}

    // Override in subclasses for custom mitigation (e.g. shield)
    protected virtual float CalculateMitigation(DamageInfo info)
    {
        float dmg = info.amount;
        dmg = Mathf.Max(0f, dmg - armor);               // flat reduction
        dmg *= (1f - armorPercent);                      // percent reduction
        return dmg;
    }

    // In HealthComponent
public virtual void ResetHealth()
{
    isDead = false;
    currentHealth = maxHealth;
    events.OnHealthChanged?.Invoke(currentHealth, maxHealth);
}
    // Hooks for subclasses — no need to call base unless chaining
    protected virtual void OnAfterDamage(DamageInfo info) { }
    protected virtual void OnDeath() { }
}