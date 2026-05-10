using UnityEngine;

public enum DamageType { Physical, Fire, Poison, Magic, Fall }

[System.Serializable]
public struct DamageInfo
{
    public float amount;
    public DamageType type;
    public GameObject source;       
    public Vector3 hitPoint;        
    public Vector3 knockbackDir;    
    public float knockbackForce;
    public bool isCritical;

    // Factory method for quick construction
    public static DamageInfo Create(float amount, GameObject source,
        DamageType type = DamageType.Physical, bool isCritical = false)
    {
        return new DamageInfo
        {
            amount = amount,
            source = source,
            type = type,
            isCritical = isCritical,
            knockbackDir = Vector3.zero,
            knockbackForce = 0f,
            hitPoint = Vector3.zero
        };
    }
}