using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapSystem : MonoBehaviour
{
    [Header("References")]
    public Camera minimapCamera;
    public RawImage minimapDisplay;
    public Transform player;
    public RectTransform playerIcon;
    
    [Header("Settings")]
    public float minimapSize = 100f;
    public float zoomLevel = 20f;
    public bool rotateWithPlayer = true;
    public Color playerIconColor = Color.blue;
    public Color enemyIconColor = Color.red;
    
    [Header("Icons")]
    public GameObject enemyIconPrefab;
    public Transform iconContainer;
    
    // Private variables
    private Dictionary<Transform, RectTransform> enemyIcons = new Dictionary<Transform, RectTransform>();
    private List<Transform> enemies = new List<Transform>();
    private RenderTexture minimapTexture;
    private float minimapRatio;
    
    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        // Set up minimap camera if assigned
        if (minimapCamera != null)
        {
            // Create render texture
            minimapTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            minimapTexture.Create();
            
            // Set up camera
            minimapCamera.targetTexture = minimapTexture;
            minimapCamera.orthographicSize = zoomLevel;
            
            // Assign texture to UI display
            if (minimapDisplay != null)
            {
                minimapDisplay.texture = minimapTexture;
                minimapRatio = minimapDisplay.rectTransform.rect.width / minimapDisplay.rectTransform.rect.height;
            }
        }
        
        // Set player icon color
        if (playerIcon != null)
        {
            Image iconImage = playerIcon.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.color = playerIconColor;
            }
        }
        
        // Initial enemy search
        FindEnemies();
    }
    
    void LateUpdate()
    {
        // Skip if no player or camera
        if (player == null || minimapCamera == null) return;
        
        // Update camera position to follow player
        Vector3 newPosition = player.position;
        newPosition.y = minimapCamera.transform.position.y; // Keep height
        minimapCamera.transform.position = newPosition;
        
        // Rotate camera with player if enabled
        if (rotateWithPlayer)
        {
            minimapCamera.transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
            
            // Keep player icon pointing up
            if (playerIcon != null)
            {
                playerIcon.rotation = Quaternion.identity;
            }
        }
        
        // Update enemy icons
        UpdateEnemyIcons();
        
        // Periodically search for new enemies
        if (Time.frameCount % 60 == 0) // Every 60 frames
        {
            FindEnemies();
        }
    }
    
    void FindEnemies()
    {
        // Find all enemies in the scene
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemyObj in enemyObjects)
        {
            Transform enemyTransform = enemyObj.transform;
            
            // Skip if already tracked
            if (enemies.Contains(enemyTransform))
                continue;
            
            // Add to tracking list
            enemies.Add(enemyTransform);
            
            // Create icon for this enemy
            CreateEnemyIcon(enemyTransform);
        }
    }
    
    void CreateEnemyIcon(Transform enemy)
    {
        // Skip if no prefab or container
        if (enemyIconPrefab == null || iconContainer == null) return;
        
        // Create icon
        GameObject iconObj = Instantiate(enemyIconPrefab, iconContainer);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        
        // Set color
        Image iconImage = iconObj.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.color = enemyIconColor;
        }
        
        // Add to dictionary
        enemyIcons[enemy] = iconRect;
    }
    
    void UpdateEnemyIcons()
    {
        // Update icon positions based on enemy world positions
        List<Transform> deadEnemies = new List<Transform>();
        
        foreach (var pair in enemyIcons)
        {
            Transform enemy = pair.Key;
            RectTransform icon = pair.Value;
            
            // Check if enemy still exists
            if (enemy == null)
            {
                deadEnemies.Add(enemy);
                continue;
            }
            
            // Calculate screen position
            Vector3 enemyPos = enemy.position;
            
            // Skip enemies too far from player (outside minimap range)
            float distance = Vector3.Distance(new Vector3(player.position.x, 0, player.position.z),
                                             new Vector3(enemyPos.x, 0, enemyPos.z));
            
            if (distance > zoomLevel * 2)
            {
                icon.gameObject.SetActive(false);
                continue;
            }
            
            // Calculate relative position
            Vector3 directionFromPlayer = enemyPos - player.position;
            
            // If rotating with player, adjust direction
            if (rotateWithPlayer)
            {
                directionFromPlayer = Quaternion.Euler(0, -player.eulerAngles.y, 0) * directionFromPlayer;
            }
            
            // Scale direction to fit minimap
            Vector2 iconPos = new Vector2(
                directionFromPlayer.x / (zoomLevel * 2) * minimapSize,
                directionFromPlayer.z / (zoomLevel * 2) * minimapSize
            );
            
            // Apply position to icon
            icon.anchoredPosition = iconPos;
            icon.gameObject.SetActive(true);
        }
        
        // Clean up destroyed enemies
        foreach (Transform deadEnemy in deadEnemies)
        {
            if (enemyIcons.TryGetValue(deadEnemy, out RectTransform icon))
            {
                if (icon != null)
                {
                    Destroy(icon.gameObject);
                }
                enemyIcons.Remove(deadEnemy);
            }
            enemies.Remove(deadEnemy);
        }
    }
    
    public void SetZoom(float newZoomLevel)
    {
        zoomLevel = Mathf.Clamp(newZoomLevel, 10f, 100f);
        
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = zoomLevel;
        }
    }
    
    public void ToggleRotation()
    {
        rotateWithPlayer = !rotateWithPlayer;
    }
    
    void OnDestroy()
    {
        // Clean up render texture
        if (minimapTexture != null)
        {
            minimapTexture.Release();
            Destroy(minimapTexture);
        }
    }
}
