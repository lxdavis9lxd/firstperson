using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileInputHandler : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public WeaponSystem weaponSystem;

    [Header("Joystick Settings")]
    public RectTransform leftJoystickArea;
    public RectTransform leftJoystickBackground;
    public RectTransform leftJoystickHandle;
    public float joystickRadius = 50f;

    [Header("Look Control Settings")]
    public RectTransform lookArea;
    public float lookSensitivity = 0.5f;
    public bool invertYAxis = false;

    [Header("Buttons")]
    public Button shootButton;
    public Button jumpButton;
    public Button reloadButton;
    public Button pauseButton;
    public Button[] weaponButtons;

    // Private variables
    private bool joystickActive = false;
    private int joystickPointerId = -1;
    private Vector2 joystickInput = Vector2.zero;
    
    private bool lookActive = false;
    private int lookPointerId = -1;
    private Vector2 lookStartPosition;

    private bool jumpPressed = false;
    private bool shootPressed = false;
    private bool reloadPressed = false;

    void Start()
    {
        // Check if we have a player controller
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        // Check if we have a weapon system
        if (weaponSystem == null)
        {
            weaponSystem = FindObjectOfType<WeaponSystem>();
        }

        // Set up button events
        if (shootButton != null)
        {
            shootButton.onClick.AddListener(OnShootButtonPressed);
        }

        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJumpButtonPressed);
        }

        if (reloadButton != null)
        {
            reloadButton.onClick.AddListener(OnReloadButtonPressed);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseButtonPressed);
        }

        // Set up weapon buttons
        for (int i = 0; i < weaponButtons.Length; i++)
        {
            int weaponIndex = i; // Create a local copy for the closure
            if (weaponButtons[i] != null)
            {
                weaponButtons[i].onClick.AddListener(() => OnWeaponButtonPressed(weaponIndex));
            }
        }

        // Load sensitivity from player prefs
        lookSensitivity = PlayerPrefs.GetFloat("Sensitivity", lookSensitivity);
    }

    void Update()
    {
        // Process touch input
        ProcessTouches();

        // Apply joystick input to player movement
        if (playerController != null)
        {
            playerController.SetMovementInput(joystickInput);
        }

        // Reset jump button state
        jumpPressed = false;
    }

    void ProcessTouches()
    {
        // Handle all touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // Handle touch based on phase
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    ProcessTouchBegan(touch);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    ProcessTouchMoved(touch);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    ProcessTouchEnded(touch);
                    break;
            }
        }

        // Reset input if no touches
        if (Input.touchCount == 0)
        {
            joystickActive = false;
            lookActive = false;
            joystickInput = Vector2.zero;
            
            // Reset joystick handle position
            if (leftJoystickHandle != null)
            {
                leftJoystickHandle.anchoredPosition = Vector2.zero;
            }
        }
    }

    void ProcessTouchBegan(Touch touch)
    {
        // Check if touch is over UI element first
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return; // Don't process touches over UI buttons
        }

        // Check for joystick area
        if (!joystickActive && leftJoystickArea != null && RectTransformUtility.RectangleContainsScreenPoint(leftJoystickArea, touch.position))
        {
            joystickActive = true;
            joystickPointerId = touch.fingerId;
            
            // Move joystick to touch position
            if (leftJoystickBackground != null)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    leftJoystickArea, touch.position, null, out localPoint);
                leftJoystickBackground.anchoredPosition = localPoint;
            }
            
            return;
        }

        // Check for look area
        if (!lookActive && lookArea != null && RectTransformUtility.RectangleContainsScreenPoint(lookArea, touch.position))
        {
            lookActive = true;
            lookPointerId = touch.fingerId;
            lookStartPosition = touch.position;
            return;
        }
    }

    void ProcessTouchMoved(Touch touch)
    {
        // Process joystick
        if (joystickActive && touch.fingerId == joystickPointerId)
        {
            // Get local position within joystick area
            if (leftJoystickBackground != null && leftJoystickHandle != null)
            {
                Vector2 touchPos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    leftJoystickArea, touch.position, null, out touchPos))
                {
                    // Calculate joystick direction
                    Vector2 backgroundPos = leftJoystickBackground.anchoredPosition;
                    Vector2 direction = touchPos - backgroundPos;
                    
                    // Clamp to joystick radius
                    if (direction.magnitude > joystickRadius)
                    {
                        direction = direction.normalized * joystickRadius;
                    }
                    
                    // Move handle
                    leftJoystickHandle.anchoredPosition = direction;
                    
                    // Calculate input (normalized)
                    joystickInput = direction / joystickRadius;
                }
            }
            return;
        }

        // Process look control
        if (lookActive && touch.fingerId == lookPointerId)
        {
            if (playerController != null)
            {
                // Calculate delta from last position
                Vector2 lookDelta = touch.position - lookStartPosition;
                
                // Apply sensitivity
                lookDelta *= lookSensitivity;
                
                // Invert Y axis if needed
                if (invertYAxis)
                {
                    lookDelta.y = -lookDelta.y;
                }
                
                // Send to player controller
                playerController.SetLookInput(lookDelta.x, lookDelta.y);
                
                // Update start position for next frame
                lookStartPosition = touch.position;
            }
            return;
        }
    }

    void ProcessTouchEnded(Touch touch)
    {
        // Reset joystick
        if (joystickActive && touch.fingerId == joystickPointerId)
        {
            joystickActive = false;
            joystickInput = Vector2.zero;
            
            // Reset joystick handle position
            if (leftJoystickHandle != null)
            {
                leftJoystickHandle.anchoredPosition = Vector2.zero;
            }
        }

        // Reset look control
        if (lookActive && touch.fingerId == lookPointerId)
        {
            lookActive = false;
        }
    }

    // Button event handlers
    void OnShootButtonPressed()
    {
        shootPressed = true;
        if (weaponSystem != null)
        {
            weaponSystem.OnShootButtonPressed();
        }
    }

    void OnJumpButtonPressed()
    {
        jumpPressed = true;
        if (playerController != null)
        {
            playerController.Jump();
        }
    }

    void OnReloadButtonPressed()
    {
        reloadPressed = true;
        if (weaponSystem != null)
        {
            weaponSystem.OnReloadButtonPressed();
        }
    }

    void OnPauseButtonPressed()
    {
        // Find game manager and toggle pause
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.TogglePause();
        }
    }

    void OnWeaponButtonPressed(int weaponIndex)
    {
        if (weaponSystem != null)
        {
            weaponSystem.OnSwitchWeaponButtonPressed(weaponIndex);
        }
    }

    // Accessors for player controller
    public bool IsJumpPressed()
    {
        return jumpPressed;
    }

    public bool IsShootPressed()
    {
        return shootPressed;
    }

    public bool IsReloadPressed()
    {
        return reloadPressed;
    }

    public Vector2 GetMovementInput()
    {
        return joystickInput;
    }
}
