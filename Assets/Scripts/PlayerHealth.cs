using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;
    
    [Header("UI")]
    public Slider healthSlider;
    public Image damageEffect;
    public float damageEffectDuration = 0.5f;
    
    [Header("Game Over")]
    public GameObject gameOverPanel;
    
    private float damageEffectTimer;
    private bool isDead = false;
    
    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Set up UI
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        
        // Hide game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Reset damage effect
        if (damageEffect != null)
        {
            Color color = damageEffect.color;
            color.a = 0f;
            damageEffect.color = color;
        }
    }
    
    void Update()
    {
        // Handle damage effect fade out
        if (damageEffect != null && damageEffectTimer > 0)
        {
            damageEffectTimer -= Time.deltaTime;
            
            // Calculate alpha based on timer
            float alpha = Mathf.Clamp01(damageEffectTimer / damageEffectDuration);
            
            // Set alpha of damage effect
            Color color = damageEffect.color;
            color.a = alpha;
            damageEffect.color = color;
        }
    }
    
    public void TakeDamage(int damage)
    {
        // Skip if already dead
        if (isDead) return;
        
        // Reduce health
        currentHealth -= damage;
        
        // Update health slider
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        
        // Show damage effect
        if (damageEffect != null)
        {
            damageEffectTimer = damageEffectDuration;
            Color color = damageEffect.color;
            color.a = 1f;
            damageEffect.color = color;
        }
        
        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void HealHealth(int healAmount)
    {
        // Skip if dead
        if (isDead) return;
        
        // Increase health but don't exceed max
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        
        // Update health slider
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
    
    void Die()
    {
        // Set dead flag
        isDead = true;
        
        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Disable player controller
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Disable character controller
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Enable cursor for game over menu (if in editor or standalone)
        #if UNITY_EDITOR || UNITY_STANDALONE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        #endif
    }
}
