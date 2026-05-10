using System;
using UnityEngine;

public enum HealthEventType { Damaged, Healed, Died, Staggered }

public struct HealthEvent
{
    public GameObject target;
    public DamageInfo info;
    public HealthEventType type;
    public float remainingHealth;
}

public static class HealthEventBus
{
    public static event Action<HealthEvent> OnHealthEvent;

    public static void Publish(HealthEvent e)
    {
        OnHealthEvent?.Invoke(e);
    }

    public static void Subscribe(Action<HealthEvent> handler)
    {
        OnHealthEvent += handler;
    }

    public static void Unsubscribe(Action<HealthEvent> handler)
    {
        OnHealthEvent -= handler;
    }
}