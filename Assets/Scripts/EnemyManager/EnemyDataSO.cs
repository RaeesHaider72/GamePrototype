using UnityEngine;

public enum AIBehaviourType { Chase, Patrol, ChaseAndPatrol }

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Identity")]
    public string enemyName;
    public EnemyType enemyType;
    public GameObject prefab;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float armor = 0f;
    public float moveSpeed = 3.5f;
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float detectionRange = 12f;

    [Header("AI")]
    public AIBehaviourType behaviourType = AIBehaviourType.Chase;

    [Header("Rewards")]
    public int xpReward = 50;
    public int coinReward = 10;

    [Header("Pool")]
    public int prewarmCount = 3;
}