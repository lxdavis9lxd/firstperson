using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int damageAmount = 10;
    
    [Header("AI")]
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    
    // References
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private bool isDead = false;
    
    // States
    enum State { Idle, Chasing, Attacking, Dead }
    private State currentState;
    
    void Start()
    {
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Get components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Set initial health
        currentHealth = maxHealth;
        
        // Set initial state
        ChangeState(State.Idle);
    }
    
    void Update()
    {
        // Skip AI if dead
        if (isDead) return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // State machine
        switch (currentState)
        {
            case State.Idle:
                // Check if player is in detection range
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(State.Chasing);
                }
                break;
                
            case State.Chasing:
                // Set destination to player position
                agent.SetDestination(player.position);
                
                // Check if close enough to attack
                if (distanceToPlayer <= attackRange)
                {
                    ChangeState(State.Attacking);
                }
                // Go back to idle if player is too far
                else if (distanceToPlayer > detectionRange)
                {
                    ChangeState(State.Idle);
                }
                break;
                
            case State.Attacking:
                // Face the player
                FaceTarget();
                
                // Perform attack
                Attack();
                
                // Check if player moved out of attack range
                if (distanceToPlayer > attackRange)
                {
                    ChangeState(State.Chasing);
                }
                break;
        }
    }
    
    void ChangeState(State newState)
    {
        // Exit current state
        switch (currentState)
        {
            case State.Attacking:
                // Reset attack timer
                break;
        }
        
        // Set new state
        currentState = newState;
        
        // Enter new state
        switch (currentState)
        {
            case State.Idle:
                agent.isStopped = true;
                // Play idle animation
                if (animator != null) animator.SetTrigger("Idle");
                break;
                
            case State.Chasing:
                agent.isStopped = false;
                agent.speed = moveSpeed;
                // Play run animation
                if (animator != null) animator.SetTrigger("Run");
                break;
                
            case State.Attacking:
                agent.isStopped = true;
                // Play attack animation
                if (animator != null) animator.SetTrigger("Attack");
                break;
                
            case State.Dead:
                agent.isStopped = true;
                // Play death animation
                if (animator != null) animator.SetTrigger("Die");
                break;
        }
    }
    
    void FaceTarget()
    {
        // Calculate direction to player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Keep on same horizontal plane
        
        // Face the direction
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }
    }
    
    void Attack()
    {
        // This would be called by animation event or on a timer
        // For now, let's just check if player is still in range and damage them
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            // Get player health component
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            
            // Damage player if component exists
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        // Skip if already dead
        if (isDead) return;
        
        // Reduce health
        currentHealth -= damage;
        
        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Play hit animation/effect
            if (animator != null) animator.SetTrigger("Hit");
        }
    }
    
    void Die()
    {
        // Set dead flag
        isDead = true;
        
        // Change state to dead
        ChangeState(State.Dead);
        
        // Disable collider
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null) enemyCollider.enabled = false;
        
        // Disable NavMeshAgent
        if (agent != null) agent.enabled = false;
        
        // Destroy gameObject after delay (or could be used for respawning)
        Destroy(gameObject, 3f);
    }
}
