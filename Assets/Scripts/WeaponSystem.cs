using UnityEngine;
using TMPro;

public class WeaponSystem : MonoBehaviour
{
    [System.Serializable]
    public class Weapon
    {
        public string name;
        public GameObject model;
        public int damage = 10;
        public float fireRate = 0.5f;
        public int maxAmmo = 30;
        public int currentAmmo;
        public float reloadTime = 1.5f;
        public bool isAutomatic = false;
        public AudioClip shootSound;
        public AudioClip reloadSound;
        public GameObject muzzleFlash;
        public float range = 100f;
        
        [HideInInspector]
        public float nextFireTime;
    }
    
    [Header("Weapons")]
    public Weapon[] weapons;
    public int currentWeaponIndex = 0;
    
    [Header("References")]
    public Camera playerCamera;
    public Transform shootPoint;
    public ParticleSystem impactEffect;
    public TextMeshProUGUI ammoText;
    
    [Header("Crosshair")]
    public RectTransform crosshair;
    public float crosshairSize = 20f;
    public float crosshairExpandedSize = 40f;
    
    private AudioSource audioSource;
    private bool isReloading = false;
    private float crosshairReturnSpeed = 5f;
    
    void Start()
    {
        // Get components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize weapons
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].currentAmmo = weapons[i].maxAmmo;
            
            // Disable all weapon models initially
            if (weapons[i].model != null)
            {
                weapons[i].model.SetActive(false);
            }
        }
        
        // Show initial weapon
        SwitchWeapon(currentWeaponIndex);
        
        // Update ammo UI
        UpdateAmmoUI();
    }
    
    void Update()
    {
        // Skip if no weapons
        if (weapons.Length == 0) return;
        
        // Get current weapon
        Weapon currentWeapon = weapons[currentWeaponIndex];
        
        // Handle inputs for PC testing
        #if UNITY_EDITOR || UNITY_STANDALONE
        // Switch weapons with number keys
        for (int i = 0; i < Mathf.Min(weapons.Length, 9); i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                SwitchWeapon(i);
            }
        }
        
        // Weapon scroll
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel > 0f)
        {
            int newIndex = (currentWeaponIndex + 1) % weapons.Length;
            SwitchWeapon(newIndex);
        }
        else if (scrollWheel < 0f)
        {
            int newIndex = (currentWeaponIndex - 1 + weapons.Length) % weapons.Length;
            SwitchWeapon(newIndex);
        }
        
        // Shooting
        if (currentWeapon.isAutomatic)
        {
            if (Input.GetButton("Fire1"))
            {
                TryShoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                TryShoot();
            }
        }
        
        // Reload
        if (Input.GetKeyDown(KeyCode.R) || currentWeapon.currentAmmo == 0)
        {
            TryReload();
        }
        #endif
        
        // Smoothly return crosshair to normal size
        if (crosshair != null)
        {
            crosshair.sizeDelta = Vector2.Lerp(crosshair.sizeDelta, 
                new Vector2(crosshairSize, crosshairSize), 
                Time.deltaTime * crosshairReturnSpeed);
        }
    }
    
    void SwitchWeapon(int index)
    {
        // Validate index
        if (index < 0 || index >= weapons.Length) return;
        
        // Disable current weapon model
        if (weapons[currentWeaponIndex].model != null)
        {
            weapons[currentWeaponIndex].model.SetActive(false);
        }
        
        // Update index
        currentWeaponIndex = index;
        
        // Enable new weapon model
        if (weapons[currentWeaponIndex].model != null)
        {
            weapons[currentWeaponIndex].model.SetActive(true);
        }
        
        // Cancel reloading
        isReloading = false;
        
        // Update ammo UI
        UpdateAmmoUI();
    }
    
    void TryShoot()
    {
        // Get current weapon
        Weapon currentWeapon = weapons[currentWeaponIndex];
        
        // Check if can shoot
        if (isReloading) return;
        if (Time.time < currentWeapon.nextFireTime) return;
        if (currentWeapon.currentAmmo <= 0)
        {
            TryReload();
            return;
        }
        
        // Set next fire time
        currentWeapon.nextFireTime = Time.time + currentWeapon.fireRate;
        
        // Reduce ammo
        currentWeapon.currentAmmo--;
        
        // Update ammo UI
        UpdateAmmoUI();
        
        // Play shoot sound
        if (currentWeapon.shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(currentWeapon.shootSound);
        }
        
        // Show muzzle flash
        if (currentWeapon.muzzleFlash != null)
        {
            GameObject flash = Instantiate(currentWeapon.muzzleFlash, shootPoint.position, shootPoint.rotation);
            Destroy(flash, 0.1f);
        }
        
        // Expand crosshair
        if (crosshair != null)
        {
            crosshair.sizeDelta = new Vector2(crosshairExpandedSize, crosshairExpandedSize);
        }
        
        // Perform raycast for hit detection
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, currentWeapon.range))
        {
            // Check if hit an enemy
            if (hit.collider.CompareTag("Enemy"))
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(currentWeapon.damage);
                }
            }
            
            // Show impact effect
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }
    
    void TryReload()
    {
        // Get current weapon
        Weapon currentWeapon = weapons[currentWeaponIndex];
        
        // Skip if already reloading or ammo is full
        if (isReloading) return;
        if (currentWeapon.currentAmmo == currentWeapon.maxAmmo) return;
        
        // Start reloading
        isReloading = true;
        
        // Play reload sound
        if (currentWeapon.reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(currentWeapon.reloadSound);
        }
        
        // Schedule reload completion
        Invoke("CompleteReload", currentWeapon.reloadTime);
    }
    
    void CompleteReload()
    {
        // End reloading
        isReloading = false;
        
        // Refill ammo
        weapons[currentWeaponIndex].currentAmmo = weapons[currentWeaponIndex].maxAmmo;
        
        // Update UI
        UpdateAmmoUI();
    }
    
    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            Weapon currentWeapon = weapons[currentWeaponIndex];
            ammoText.text = $"{currentWeapon.currentAmmo} / {currentWeapon.maxAmmo}";
        }
    }
    
    // Called from UIManager when shoot button is pressed
    public void OnShootButtonPressed()
    {
        TryShoot();
    }
    
    // Called from UIManager when reload button is pressed
    public void OnReloadButtonPressed()
    {
        TryReload();
    }
    
    // Called from UIManager when weapon switch buttons are pressed
    public void OnSwitchWeaponButtonPressed(int index)
    {
        SwitchWeapon(index);
    }
}
