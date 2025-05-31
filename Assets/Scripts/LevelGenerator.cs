using UnityEngine;
using UnityEngine.AI;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Generation")]
    public int levelWidth = 10;
    public int levelLength = 10;
    public float roomSize = 5f;
    
    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    public GameObject enemyPrefab;
    public GameObject healthPickupPrefab;
    public GameObject ammoPickupPrefab;
    
    [Header("Enemy Spawning")]
    public int minEnemies = 5;
    public int maxEnemies = 15;
    
    [Header("Pickup Spawning")]
    public int healthPickups = 3;
    public int ammoPickups = 5;
    
    [Header("Player")]
    public GameObject playerPrefab;
    public Vector3 playerStartPosition = new Vector3(2, 1, 2);
    
    private Transform levelParent;
    private bool[,] levelLayout;
    private NavMeshSurface navMeshSurface;
    
    void Start()
    {
        // Create parent for level objects
        GameObject levelParentObj = new GameObject("LevelParent");
        levelParent = levelParentObj.transform;
        
        // Add NavMeshSurface component to bake navmesh
        navMeshSurface = levelParentObj.AddComponent<NavMeshSurface>();
        
        // Generate level
        GenerateLevel();
        
        // Bake navmesh
        navMeshSurface.BuildNavMesh();
        
        // Spawn player
        SpawnPlayer();
        
        // Spawn enemies
        SpawnEnemies();
        
        // Spawn pickups
        SpawnPickups();
    }
    
    void GenerateLevel()
    {
        // Initialize level layout
        levelLayout = new bool[levelWidth, levelLength];
        
        // Generate random layout
        GenerateRandomLayout();
        
        // Create floors and walls
        for (int x = 0; x < levelWidth; x++)
        {
            for (int z = 0; z < levelLength; z++)
            {
                // Position for this cell
                Vector3 position = new Vector3(x * roomSize, 0, z * roomSize);
                
                // Create floor if this is a room
                if (levelLayout[x, z])
                {
                    // Create floor
                    GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity, levelParent);
                    floor.transform.localScale = new Vector3(roomSize, 1, roomSize);
                    
                    // Check and create walls
                    CheckAndCreateWall(x, z, x + 1, z, Vector3.right, Vector3.forward);
                    CheckAndCreateWall(x, z, x - 1, z, Vector3.left, Vector3.forward);
                    CheckAndCreateWall(x, z, x, z + 1, Vector3.forward, Vector3.right);
                    CheckAndCreateWall(x, z, x, z - 1, Vector3.back, Vector3.right);
                }
            }
        }
    }
    
    void GenerateRandomLayout()
    {
        // Start with all walls
        for (int x = 0; x < levelWidth; x++)
        {
            for (int z = 0; z < levelLength; z++)
            {
                levelLayout[x, z] = false;
            }
        }
        
        // Create a simple layout with a main path
        int middleX = levelWidth / 2;
        int middleZ = levelLength / 2;
        
        // Create main corridor
        for (int x = 1; x < levelWidth - 1; x++)
        {
            levelLayout[x, middleZ] = true;
        }
        
        // Create cross corridor
        for (int z = 1; z < levelLength - 1; z++)
        {
            levelLayout[middleX, z] = true;
        }
        
        // Add some random rooms
        int roomCount = Random.Range(5, 10);
        for (int i = 0; i < roomCount; i++)
        {
            int roomX = Random.Range(1, levelWidth - 1);
            int roomZ = Random.Range(1, levelLength - 1);
            
            // Create room and maybe a corridor to it
            levelLayout[roomX, roomZ] = true;
            
            // Connect to nearest corridor
            ConnectToMainPath(roomX, roomZ);
        }
        
        // Make sure player starting position is valid
        int startX = Mathf.FloorToInt(playerStartPosition.x / roomSize);
        int startZ = Mathf.FloorToInt(playerStartPosition.z / roomSize);
        
        // Ensure start position is within bounds
        startX = Mathf.Clamp(startX, 1, levelWidth - 2);
        startZ = Mathf.Clamp(startZ, 1, levelLength - 2);
        
        // Make sure there is a room at the start position
        levelLayout[startX, startZ] = true;
        
        // Update player start position
        playerStartPosition = new Vector3(startX * roomSize, 1, startZ * roomSize);
    }
    
    void ConnectToMainPath(int roomX, int roomZ)
    {
        // Connect to main corridor
        int middleX = levelWidth / 2;
        int middleZ = levelLength / 2;
        
        // Decide whether to connect horizontally or vertically first
        if (Random.value < 0.5f)
        {
            // Connect horizontally then vertically
            for (int x = Mathf.Min(roomX, middleX); x <= Mathf.Max(roomX, middleX); x++)
            {
                levelLayout[x, roomZ] = true;
            }
            
            for (int z = Mathf.Min(roomZ, middleZ); z <= Mathf.Max(roomZ, middleZ); z++)
            {
                levelLayout[middleX, z] = true;
            }
        }
        else
        {
            // Connect vertically then horizontally
            for (int z = Mathf.Min(roomZ, middleZ); z <= Mathf.Max(roomZ, middleZ); z++)
            {
                levelLayout[roomX, z] = true;
            }
            
            for (int x = Mathf.Min(roomX, middleX); x <= Mathf.Max(roomX, middleX); x++)
            {
                levelLayout[x, middleZ] = true;
            }
        }
    }
    
    void CheckAndCreateWall(int x1, int z1, int x2, int z2, Vector3 direction, Vector3 wallDirection)
    {
        // Check if the neighboring cell is a room or out of bounds
        bool createWall = true;
        
        if (x2 >= 0 && x2 < levelWidth && z2 >= 0 && z2 < levelLength)
        {
            if (levelLayout[x2, z2])
            {
                // If neighboring cell is also a room, add a door instead of a wall
                createWall = false;
                
                // Randomly decide if we should have a door (50% chance)
                if (Random.value < 0.5f && doorPrefab != null)
                {
                    Vector3 position = new Vector3(x1 * roomSize, 0, z1 * roomSize) + direction * roomSize / 2;
                    GameObject door = Instantiate(doorPrefab, position, Quaternion.LookRotation(direction), levelParent);
                    door.transform.localScale = new Vector3(roomSize, roomSize, 1);
                }
            }
        }
        
        if (createWall && wallPrefab != null)
        {
            Vector3 position = new Vector3(x1 * roomSize, 0, z1 * roomSize) + direction * roomSize / 2;
            GameObject wall = Instantiate(wallPrefab, position, Quaternion.LookRotation(direction), levelParent);
            wall.transform.localScale = new Vector3(roomSize, roomSize, 1);
        }
    }
    
    void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            // Adjust height to prevent falling through floor
            Vector3 spawnPosition = playerStartPosition + Vector3.up;
            
            // Create player
            Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        }
    }
    
    void SpawnEnemies()
    {
        if (enemyPrefab == null) return;
        
        // Determine number of enemies to spawn
        int enemiesToSpawn = Random.Range(minEnemies, maxEnemies + 1);
        
        // Update GameManager enemy count
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.enemiesRemainingToWin = enemiesToSpawn;
        }
        
        // Spawn enemies
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // Find a random room that's not the player's starting room
            int enemyX, enemyZ;
            float distanceFromPlayer;
            
            do
            {
                enemyX = Random.Range(0, levelWidth);
                enemyZ = Random.Range(0, levelLength);
                
                // Calculate distance from player start
                Vector3 enemyPos = new Vector3(enemyX * roomSize, 1, enemyZ * roomSize);
                distanceFromPlayer = Vector3.Distance(enemyPos, playerStartPosition);
                
            } while (!levelLayout[enemyX, enemyZ] || distanceFromPlayer < roomSize * 3);
            
            // Create enemy
            Vector3 position = new Vector3(enemyX * roomSize, 1, enemyZ * roomSize);
            Instantiate(enemyPrefab, position, Quaternion.identity, levelParent);
        }
    }
    
    void SpawnPickups()
    {
        // Spawn health pickups
        SpawnPickupType(healthPickupPrefab, healthPickups);
        
        // Spawn ammo pickups
        SpawnPickupType(ammoPickupPrefab, ammoPickups);
    }
    
    void SpawnPickupType(GameObject pickupPrefab, int count)
    {
        if (pickupPrefab == null) return;
        
        for (int i = 0; i < count; i++)
        {
            // Find a random room
            int pickupX, pickupZ;
            
            do
            {
                pickupX = Random.Range(0, levelWidth);
                pickupZ = Random.Range(0, levelLength);
            } while (!levelLayout[pickupX, pickupZ]);
            
            // Create pickup
            Vector3 position = new Vector3(pickupX * roomSize, 1, pickupZ * roomSize);
            
            // Add a small random offset within the room
            position.x += Random.Range(-roomSize/3, roomSize/3);
            position.z += Random.Range(-roomSize/3, roomSize/3);
            
            Instantiate(pickupPrefab, position, Quaternion.identity, levelParent);
        }
    }
}
