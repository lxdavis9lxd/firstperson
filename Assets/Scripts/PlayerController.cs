using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Player movement variables
    public float moveSpeed = 5f;
    public float lookSensitivity = 3f;
    public float jumpForce = 5f;
    public Transform cameraTransform;
    
    // Private variables
    private CharacterController characterController;
    private float verticalLookRotation;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool isJumping;
    private Vector2 movementInput;
    private bool isShooting;
    
    // Mobile input
    private MobileInputHandler mobileInput;
    
    // Footstep sounds
    [Header("Audio")]
    public float footstepInterval = 0.5f;
    public float runningFootstepInterval = 0.3f;
    private float footstepTimer;
    
    void Start()
    {
        // Get character controller component
        characterController = GetComponent<CharacterController>();
        
        // Lock and hide cursor (for development on PC)
        #if UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        #endif
        
        // Make sure we have a camera transform assigned
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Find mobile input handler
        mobileInput = FindObjectOfType<MobileInputHandler>();
        
        // Set sensitivity from player prefs
        lookSensitivity = PlayerPrefs.GetFloat("Sensitivity", lookSensitivity);
    }
    
    void Update()
    {
        // Check if the player is on the ground
        isGrounded = characterController.isGrounded;
        
        // Handle platform-specific input
        #if UNITY_EDITOR || UNITY_STANDALONE
        HandlePCInput();
        #else
        HandleMobileInput();
        #endif
        
        // Apply movement
        MovePlayer();
        
        // Handle shooting
        if (isShooting)
        {
            Shoot();
        }
        
        // Handle footstep sounds
        HandleFootsteps();
    }
    
    void HandlePCInput()
    {
        // Movement input (WASD or arrow keys)
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        // Store movement input
        movementInput = new Vector2(horizontalInput, verticalInput);
        
        // Calculate movement direction relative to where the player is looking
        moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        
        // Look input (mouse movement)
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;
        
        // Apply look input
        ApplyLookRotation(mouseX, mouseY);
        
        // Jump input
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
        
        // Shooting input
        isShooting = Input.GetButtonDown("Fire1");
    }
    
    void HandleMobileInput()
    {
        if (mobileInput != null)
        {
            // Get movement input from mobile input handler
            Vector2 input = mobileInput.GetMovementInput();
            movementInput = input;
            
            // Calculate movement direction from mobile input
            moveDirection = transform.forward * input.y + transform.right * input.x;
            
            // Check for jump from mobile input
            if (mobileInput.IsJumpPressed() && isGrounded)
            {
                Jump();
            }
            
            // Check for shoot from mobile input
            isShooting = mobileInput.IsShootPressed();
        }
    }
    
    void ApplyLookRotation(float mouseX, float mouseY)
    {
        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);
        
        // Rotate camera vertically (with clamping to prevent over-rotation)
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }
    
    public void SetLookInput(float horizontal, float vertical)
    {
        ApplyLookRotation(horizontal, vertical);
    }
    
    public void SetMovementInput(Vector2 input)
    {
        movementInput = input;
        moveDirection = transform.forward * input.y + transform.right * input.x;
    }
    
    public void Jump()
    {
        if (isGrounded)
        {
            moveDirection.y = jumpForce;
            isJumping = true;
            
            // Play jump sound
            AudioManager audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.PlaySound("Jump");
            }
        }
    }
    
    void HandleFootsteps()
    {
        // Only play footsteps when moving on the ground
        if (isGrounded && movementInput.magnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            
            if (footstepTimer <= 0f)
            {
                // Determine if running or walking
                float interval = (movementInput.magnitude > 0.8f) ? runningFootstepInterval : footstepInterval;
                footstepTimer = interval;
                
                // Play footstep sound
                AudioManager audioManager = AudioManager.Instance;
                if (audioManager != null)
                {
                    audioManager.PlaySound("Footstep");
                }
            }
        }
        else
        {
            // Reset timer when not moving
            footstepTimer = 0f;
        }
    }
    
    void MovePlayer()
    {
        // Apply gravity if player is not grounded
        if (!isGrounded)
        {
            moveDirection.y += Physics.gravity.y * Time.deltaTime;
        }
        else if (moveDirection.y < 0)
        {
            // Small negative value when grounded to keep player on slopes
            moveDirection.y = -2f;
            
            // Reset jumping flag
            if (isJumping)
            {
                isJumping = false;
                
                // Play landing sound if was jumping
                AudioManager audioManager = AudioManager.Instance;
                if (audioManager != null)
                {
                    audioManager.PlaySound("Land");
                }
            }
        }
        
        // Move the player using the character controller
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
    
    void Shoot()
    {
        // Reset shooting flag
        isShooting = false;
        
        // Create raycast from camera center
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;
        
        // Check if ray hits something
        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Check if we hit an enemy
            if (hit.collider.CompareTag("Enemy"))
            {
                // Get the enemy component and damage it
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(10);
                }
            }
            
            // Create hit effect (can be implemented later)
            // Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }
}
