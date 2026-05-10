public interface IDamageable
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    bool IsAlive { get; }

    void TakeDamage(DamageInfo info);
    void Heal(float amount);
    void Die();
}