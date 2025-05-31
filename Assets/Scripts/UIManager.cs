using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Mobile Controls")]
    public GameObject mobileControls;
    public RectTransform joystickArea;
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;
    public Button shootButton;
    public Button pauseButton;
    
    [Header("UI Panels")]
    public GameObject healthPanel;
    public GameObject ammoPanel;
    
    void Start()
    {
        // Show or hide mobile controls based on platform
        #if UNITY_ANDROID || UNITY_IOS
        ShowMobileControls(true);
        #else
        ShowMobileControls(false);
        #endif
        
        // Set up button events
        if (shootButton != null)
        {
            shootButton.onClick.AddListener(OnShootButtonPressed);
        }
        
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseButtonPressed);
        }
    }
    
    void ShowMobileControls(bool show)
    {
        if (mobileControls != null)
        {
            mobileControls.SetActive(show);
        }
    }
    
    void OnShootButtonPressed()
    {
        // Find player controller and trigger shoot
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            // This would be implemented in the player controller
            // For now, we can use a message to communicate
            playerController.SendMessage("OnShootButtonPressed", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    void OnPauseButtonPressed()
    {
        // Toggle pause via game manager
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.TogglePause();
        }
    }
}
