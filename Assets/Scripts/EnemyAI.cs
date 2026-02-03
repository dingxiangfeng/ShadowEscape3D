using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// EnemyAI handles enemy behavior including patrol, chase, and attack states
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    
    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waypointTolerance = 0.5f;
    
    [Header("Combat")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int pointsOnDefeat = 250;
    
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    
    // Components
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    
    // State machine
    private EnemyState currentState = EnemyState.Patrol;
    private int currentPatrolIndex = 0;
    private float lastAttackTime;
    private float stateTimer;
    
    // Animation hashes
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DieHash = Animator.StringToHash("Die");
    
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Dead
    }
    
    private void Start()
    {
        // Get components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Initialize health
        currentHealth = maxHealth;
        
        // Start patrol
        if (patrolPoints.Length > 0)
        {
            SetState(EnemyState.Patrol);
        }
        else
        {
            SetState(EnemyState.Idle);
        }
    }
    
    private void Update()
    {
        if (currentState == EnemyState.Dead) return;
        
        // Update state machine
        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
        }
        
        // Update animations
        UpdateAnimations();
    }
    
    /// <summary>
    /// Updates idle state behavior
    /// </summary>
    private void UpdateIdle()
    {
        // Check for player detection
        if (CanSeePlayer())
        {
            SetState(EnemyState.Chase);
        }
    }
    
    /// <summary>
    /// Updates patrol state behavior
    /// </summary>
    private void UpdatePatrol()
    {
        // Check for player detection
        if (CanSeePlayer())
        {
            SetState(EnemyState.Chase);
            return;
        }
        
        // Move to current patrol point
        if (patrolPoints.Length > 0)
        {
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            
            // Check if reached patrol point
            if (Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex].position) < waypointTolerance)
            {
                // Move to next patrol point
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }
    }
    
    /// <summary>
    /// Updates chase state behavior
    /// </summary>
    private void UpdateChase()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if player is in attack range
        if (distanceToPlayer <= attackRange)
        {
            SetState(EnemyState.Attack);
            return;
        }
        
        // Check if player escaped
        if (distanceToPlayer > detectionRange * 1.5f || !CanSeePlayer())
        {
            stateTimer += Time.deltaTime;
            if (stateTimer > 3f) // Lost player for 3 seconds
            {
                SetState(EnemyState.Patrol);
                return;
            }
        }
        else
        {
            stateTimer = 0f;
        }
        
        // Chase player
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }
    
    /// <summary>
    /// Updates attack state behavior
    /// </summary>
    private void UpdateAttack()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if player moved out of attack range
        if (distanceToPlayer > attackRange * 1.2f)
        {
            SetState(EnemyState.Chase);
            return;
        }
        
        // Face the player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                Quaternion.LookRotation(direction), 10f * Time.deltaTime);
        }
        
        // Stop moving
        agent.SetDestination(transform.position);
        
        // Attack if cooldown is ready
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
        }
    }
    
    /// <summary>
    /// Performs an attack on the player
    /// </summary>
    private void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger(AttackHash);
        }
        
        // Deal damage to player (implement player health system)
        // PlayerHealth.Instance?.TakeDamage(damage);
        
        Debug.Log($"[EnemyAI] Attacked player for {damage} damage!");
    }
    
    /// <summary>
    /// Checks if the enemy can see the player
    /// </summary>
    private bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        // Check distance
        if (distanceToPlayer > detectionRange) return false;
        
        // Check field of view
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > fieldOfView / 2f) return false;
        
        // Check line of sight (raycast)
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up; // Start from eye level
        Vector3 rayDirection = (player.position + Vector3.up) - rayStart;
        
        if (Physics.Raycast(rayStart, rayDirection.normalized, out hit, detectionRange, obstacleLayer | playerLayer))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Sets the enemy state
    /// </summary>
    private void SetState(EnemyState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        
        Debug.Log($"[EnemyAI] State changed to: {newState}");
    }
    
    /// <summary>
    /// Updates animator parameters
    /// </summary>
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        float speed = agent.velocity.magnitude;
        animator.SetFloat(SpeedHash, speed);
    }
    
    /// <summary>
    /// Takes damage from an attack
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        if (currentState == EnemyState.Dead) return;
        
        currentHealth -= damageAmount;
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Alert enemy to player position
            SetState(EnemyState.Chase);
        }
    }
    
    /// <summary>
    /// Handles enemy death
    /// </summary>
    private void Die()
    {
        SetState(EnemyState.Dead);
        
        // Stop movement
        agent.enabled = false;
        
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger(DieHash);
        }
        
        // Award points
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.AddPoints(pointsOnDefeat, "Enemy Defeated");
        }
        
        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Destroy after delay
        Destroy(gameObject, 3f);
    }
    
    /// <summary>
    /// Draws gizmos for debugging
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw field of view
        Gizmos.color = Color.blue;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftBoundary * detectionRange);
        Gizmos.DrawRay(transform.position, rightBoundary * detectionRange);
    }
}
