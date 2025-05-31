using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject enemyPrefab;
    public int maxEnemies = 10;
    public float spawnInterval = 5f;
    public float minDistanceFromPlayer = 10f;
    public float maxDistanceFromPlayer = 30f;
    
    [Header("Difficulty Settings")]
    public float difficultyRamp = 0.1f; // How much to increase difficulty per spawn
    public int maxDifficultyLevel = 5;
    public float enemyHealthMultiplier = 0.2f; // How much to increase enemy health per difficulty level
    public float enemyDamageMultiplier = 0.1f; // How much to increase enemy damage per difficulty level
    public float enemySpeedMultiplier = 0.05f; // How much to increase enemy speed per difficulty level
    
    [Header("Debug")]
    public bool showSpawnPoints = true;
    
    // Private variables
    private Transform player;
    private int currentEnemyCount = 0;
    private float spawnTimer;
    private int difficultyLevel = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    void Start()
    {
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Initialize spawn timer
        spawnTimer = spawnInterval;
        
        // Adjust max enemies based on device performance
        #if UNITY_ANDROID
        if (SystemInfo.processorCount <= 4)
        {
            maxEnemies = Mathf.Min(maxEnemies, 5);
        }
        #endif
    }
    
    void Update()
    {
        // Skip if no player or enemy prefab
        if (player == null || enemyPrefab == null) return;
        
        // Update enemy count (some might have been destroyed)
        UpdateEnemyCount();
        
        // Check if we can spawn more enemies
        if (currentEnemyCount < maxEnemies)
        {
            // Update spawn timer
            spawnTimer -= Time.deltaTime;
            
            if (spawnTimer <= 0f)
            {
                // Reset timer
                spawnTimer = spawnInterval;
                
                // Try to spawn an enemy
                SpawnEnemy();
            }
        }
    }
    
    void UpdateEnemyCount()
    {
        // Remove null entries (destroyed enemies)
        activeEnemies.RemoveAll(enemy => enemy == null);
        
        // Update count
        currentEnemyCount = activeEnemies.Count;
    }
    
    void SpawnEnemy()
    {
        // Find a valid spawn position
        Vector3 spawnPosition = FindSpawnPosition();
        
        if (spawnPosition != Vector3.zero)
        {
            // Create enemy
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            
            // Apply difficulty modifications
            ApplyDifficultyToEnemy(enemy);
            
            // Add to active enemies list
            activeEnemies.Add(enemy);
            
            // Increment difficulty every few spawns
            if (currentEnemyCount % 3 == 0 && difficultyLevel < maxDifficultyLevel)
            {
                difficultyLevel++;
            }
        }
    }
    
    Vector3 FindSpawnPosition()
    {
        // Try several random positions
        for (int i = 0; i < 10; i++)
        {
            // Generate random direction around player
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // Generate random distance
            float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            
            // Calculate potential spawn position
            Vector3 potentialPosition = player.position + randomDirection * distance;
            
            // Adjust y position based on a raycast to find ground
            if (Physics.Raycast(potentialPosition + Vector3.up * 10, Vector3.down, out RaycastHit hit, 20f))
            {
                potentialPosition.y = hit.point.y + 1f; // Slightly above ground
            }
            
            // Check if position is on NavMesh
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(potentialPosition, out navHit, 5f, NavMesh.AllAreas))
            {
                // Check if position is visible to player (avoid spawning in view)
                if (!IsVisibleToPlayer(navHit.position))
                {
                    // Position is valid for spawning
                    return navHit.position;
                }
            }
        }
        
        // Failed to find valid position
        return Vector3.zero;
    }
    
    bool IsVisibleToPlayer(Vector3 position)
    {
        // Check line of sight from player camera to position
        Vector3 directionToPosition = position - player.position;
        float angle = Vector3.Angle(player.forward, directionToPosition);
        
        // If not in view angle, not visible
        if (angle > 60f) return false;
        
        // Check for obstacles between player and position
        if (Physics.Linecast(player.position + Vector3.up * 1.6f, position, out RaycastHit hit))
        {
            if (hit.point != position)
            {
                // Something is blocking the view
                return false;
            }
        }
        
        // Position is visible to player
        return true;
    }
    
    void ApplyDifficultyToEnemy(GameObject enemyObject)
    {
        // Get enemy component
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Scale health based on difficulty
            enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * (1f + difficultyLevel * enemyHealthMultiplier));
            enemy.currentHealth = enemy.maxHealth;
            
            // Scale damage based on difficulty
            enemy.damageAmount = Mathf.RoundToInt(enemy.damageAmount * (1f + difficultyLevel * enemyDamageMultiplier));
            
            // Scale speed based on difficulty
            enemy.moveSpeed = enemy.moveSpeed * (1f + difficultyLevel * enemySpeedMultiplier);
            
            // Apply modifications to NavMeshAgent
            NavMeshAgent agent = enemyObject.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = enemy.moveSpeed;
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (showSpawnPoints && player != null)
        {
            // Draw min spawn radius
            Gizmos.color = Color.green;
            DrawCircle(player.position, minDistanceFromPlayer);
            
            // Draw max spawn radius
            Gizmos.color = Color.red;
            DrawCircle(player.position, maxDistanceFromPlayer);
        }
    }
    
    void DrawCircle(Vector3 center, float radius)
    {
        int segments = 36;
        float angleStep = 360f / segments;
        
        Vector3 previousPoint = center + new Vector3(Mathf.Sin(0) * radius, 0, Mathf.Cos(0) * radius);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 nextPoint = center + new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
}
