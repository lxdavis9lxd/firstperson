using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType
    {
        Health,
        Ammo,
        Weapon
    }
    
    [Header("Pickup Settings")]
    public PickupType type;
    public int amount = 25;
    public int weaponIndex = 0; // Only used for Weapon pickups
    
    [Header("Visuals")]
    public float rotationSpeed = 50f;
    public float bobSpeed = 1f;
    public float bobHeight = 0.5f;
    
    [Header("Effects")]
    public AudioClip pickupSound;
    public GameObject pickupEffect;
    
    private Vector3 startPosition;
    
    void Start()
    {
        // Record starting position for bobbing effect
        startPosition = transform.position;
    }
    
    void Update()
    {
        // Rotate the pickup
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if player entered the trigger
        if (other.CompareTag("Player"))
        {
            // Apply pickup effect based on type
            switch (type)
            {
                case PickupType.Health:
                    ApplyHealthPickup(other.gameObject);
                    break;
                    
                case PickupType.Ammo:
                    ApplyAmmoPickup(other.gameObject);
                    break;
                    
                case PickupType.Weapon:
                    ApplyWeaponPickup(other.gameObject);
                    break;
            }
            
            // Play pickup sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            // Show pickup effect
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }
            
            // Destroy pickup
            Destroy(gameObject);
        }
    }
    
    void ApplyHealthPickup(GameObject player)
    {
        // Find player health component
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        
        // Apply health boost
        if (playerHealth != null)
        {
            playerHealth.HealHealth(amount);
        }
    }
    
    void ApplyAmmoPickup(GameObject player)
    {
        // Find weapon system component
        WeaponSystem weaponSystem = player.GetComponentInChildren<WeaponSystem>();
        
        // Apply ammo boost
        if (weaponSystem != null && weaponSystem.weapons.Length > 0)
        {
            // Add ammo to current weapon
            int currentIndex = weaponSystem.currentWeaponIndex;
            weaponSystem.weapons[currentIndex].currentAmmo += amount;
            
            // Clamp to max ammo
            weaponSystem.weapons[currentIndex].currentAmmo = 
                Mathf.Min(weaponSystem.weapons[currentIndex].currentAmmo, 
                         weaponSystem.weapons[currentIndex].maxAmmo);
        }
    }
    
    void ApplyWeaponPickup(GameObject player)
    {
        // Find weapon system component
        WeaponSystem weaponSystem = player.GetComponentInChildren<WeaponSystem>();
        
        // Apply weapon pickup
        if (weaponSystem != null && weaponSystem.weapons.Length > weaponIndex)
        {
            // Switch to the weapon
            weaponSystem.SwitchWeapon(weaponIndex);
        }
    }
}
