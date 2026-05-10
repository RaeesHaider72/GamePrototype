using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum EnemyType { Basic, Elite, Boss }

public class EnemyHealth : HealthComponent
{
    [Header("Enemy Specific")]
    [SerializeField] private EnemyType enemyType = EnemyType.Basic;
    [SerializeField] private int xpReward = 50;
    [SerializeField] private float staggerThreshold = 30f;  // damage in one hit to stagger
    [SerializeField] private float enrageThreshold = 0.25f; // % health to trigger enrage

    [Header("Enemy Events")]
    public UnityEvent OnStaggered;
    public UnityEvent OnEnraged;

    private bool hasEnraged = false;
    private Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }

    protected override void OnAfterDamage(DamageInfo info)
    {
        if (rb != null && info.knockbackForce > 0f)
            rb.AddForce(info.knockbackDir * info.knockbackForce, ForceMode.Impulse);

        if (info.amount >= staggerThreshold)
            OnStaggered?.Invoke();

        if (!hasEnraged && HealthPercent <= enrageThreshold)
        {
            hasEnraged = true;
            armor = 0f;
            OnEnraged?.Invoke();
        }
    }

    protected override void OnDeath()
    {
        Debug.Log($"{enemyType} enemy defeated. +{xpReward} XP");

        HealthEventBus.Publish(new HealthEvent
        {
            target = gameObject,
            type = HealthEventType.Died,
            remainingHealth = 0f
        });

  
    }

    public void InjectData(float hp, float armorValue)
{
    maxHealth = hp;
    armor = armorValue;
    currentHealth = maxHealth;
    hasEnraged = false; // reset for pool reuse
  ResetHealth(); // trigger health changed event for UI update
    Debug.Log($"[EnemyHealth] InjectData: maxHp={maxHealth}, armor={armor}");
}

}