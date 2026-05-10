using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyAIState { Idle, Patrol, Chase, Attack, Dead }

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    private NavMeshAgent agent;
    private EnemyHealth health;
    private Transform player;

    [Header("Runtime data — set by EnemyManager on spawn")]
    public EnemyDataSO data;
    public List<Transform> patrolWaypoints = new();

    private EnemyAIState currentState;
    private int waypointIndex;
    private float stateTimer;
    private GameObject sourcePrefab; // needed for pool return



    public Canvas healthBarCanvas; // assigned in prefab, can be enabled/disabled per enemy type    

    public GameObject deathEffect;

    // ── Lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();
    }

    // Called by EnemyManager every time this object is taken from pool
    public void Initialize(EnemyDataSO enemyData, Transform playerTransform,
    List<Transform> waypoints, GameObject prefab)
    {
        data = enemyData;
        player = playerTransform;
        patrolWaypoints = waypoints ?? new List<Transform>();
        sourcePrefab = prefab;

        agent.speed = data.moveSpeed;
        agent.stoppingDistance = data.attackRange * 0.9f;

        health.InjectData(data.maxHealth, data.armor);

        // Always unsubscribe before subscribing — prevents duplicate listeners
        // when same object is reused from pool
        UnsubscribeFromHealth();
        SubscribeToHealth();

        TransitionTo(ResolveStartState());
    }

    private void OnDisable()
    {
        UnsubscribeFromHealth();

        // Guard: only reset path if agent is active and on a NavMesh
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void Update()
    {
        if (currentState == EnemyAIState.Dead) return;
        UpdateState();
    }

    // ── State machine ──────────────────────────────────────────

    // EnemyAI.cs — replace both methods

    private void TransitionTo(EnemyAIState next)
    {
        currentState = next;
        stateTimer = 0f;

        switch (next)
        {
            case EnemyAIState.Patrol:
                agent.isStopped = false;
                // destination is already set by ResolveStartState or GoToNextWaypoint
                break;

            case EnemyAIState.Chase:
                agent.isStopped = false;
                break;

            case EnemyAIState.Attack:
                agent.isStopped = true;
                break;

            case EnemyAIState.Idle:
                agent.isStopped = true;
                break;
        }
    }

    private EnemyAIState ResolveStartState()
    {
        bool canPatrol = patrolWaypoints.Count > 0 &&
                         (data.behaviourType == AIBehaviourType.Patrol ||
                          data.behaviourType == AIBehaviourType.ChaseAndPatrol);

        if (canPatrol)
        {
            waypointIndex = Random.Range(0, patrolWaypoints.Count);
            agent.SetDestination(patrolWaypoints[waypointIndex].position);
            return EnemyAIState.Patrol;
        }

        if (data.behaviourType == AIBehaviourType.Chase)
        {
            // Chase type starts by going straight for player
            agent.isStopped = false;
            return EnemyAIState.Chase;
        }

        return EnemyAIState.Idle;
    }
    private void UpdateState()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case EnemyAIState.Idle:
                if (distToPlayer <= data.detectionRange)
                    TransitionTo(EnemyAIState.Chase);
                break;

            case EnemyAIState.Patrol:
                UpdatePatrol(distToPlayer);
                break;

            case EnemyAIState.Chase:
                UpdateChase(distToPlayer);
                break;

            case EnemyAIState.Attack:
                UpdateAttack(distToPlayer);
                break;
        }
    }

    private void UpdatePatrol(float distToPlayer)
    {
        if (distToPlayer <= data.detectionRange) { TransitionTo(EnemyAIState.Chase); return; }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            GoToNextWaypoint(); // use the method instead of duplicating logic
    }

    private void UpdateChase(float distToPlayer)
    {
        if (distToPlayer <= data.attackRange)
        {
            TransitionTo(EnemyAIState.Attack);
            return;
        }

        // Lost player — return to patrol or idle
        if (distToPlayer > data.detectionRange * 1.5f)
        {
            TransitionTo(patrolWaypoints.Count > 0
                ? EnemyAIState.Patrol
                : EnemyAIState.Idle);
            return;
        }

        agent.SetDestination(player.position);
    }

    private void UpdateAttack(float distToPlayer)
    {
        // Face player
        Vector3 dir = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(
            transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);

        if (distToPlayer > data.attackRange)
        {
            TransitionTo(EnemyAIState.Chase);
            return;
        }

        // Attack on interval
        if (stateTimer >= 1f / GetAttackRate())
        {
            stateTimer = 0f;
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        IDamageable target = player.GetComponent<IDamageable>();
        if (target == null) return;

        DamageInfo info = DamageInfo.Create(
            data.attackDamage,
            gameObject,
            DamageType.Physical
        );
        info.knockbackDir = (player.position - transform.position).normalized;
        info.knockbackForce = 4f;

        target.TakeDamage(info);
    }

    // ── Helpers ────────────────────────────────────────────────
    private void GoToNextWaypoint()
    {
        if (patrolWaypoints.Count == 0) return;
        waypointIndex = (waypointIndex + 1) % patrolWaypoints.Count;
        agent.isStopped = false;
        agent.SetDestination(patrolWaypoints[waypointIndex].position);
    }

    private float GetAttackRate() => data.enemyType switch
    {
        EnemyType.Boss => 0.8f,
        EnemyType.Elite => 1.2f,
        _ => 1.5f
    };

    // ── Health events ──────────────────────────────────────────

    private void SubscribeToHealth()
    {
        health.events.OnDeath.AddListener(OnDied);
    }

    private void UnsubscribeFromHealth()
    {
        health.events.OnDeath.RemoveListener(OnDied);
    }

    private void OnDied(DamageInfo _)
    {

            Debug.LogError($"[EnemyAI] {gameObject.name} died! State was: {currentState}");

        currentState = EnemyAIState.Dead;
        agent.isStopped = true;

        if (deathEffect != null)
        {
            deathEffect.gameObject.SetActive(true);
            deathEffect.transform.SetParent(null);
            deathEffect.transform.position = transform.position + new Vector3(0, 1.5f, 0); // unparent so it doesn't get deactivated with enemy
        }

        EnemyManager.Instance.OnEnemyDied(gameObject, sourcePrefab);
    }


}