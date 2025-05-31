using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game Settings")]
    public int enemiesRemainingToWin = 10;
    private int enemiesKilled = 0;
    
    [Header("UI")]
    public GameObject pauseMenu;
    public GameObject winPanel;
    public TMPro.TextMeshProUGUI enemyCountText;
    
    private bool isPaused = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Hide UI panels
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        
        // Update enemy count text
        UpdateEnemyCountText();
    }
    
    void Update()
    {
        // Toggle pause menu (for PC development)
        #if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        #endif
    }
    
    public void EnemyKilled()
    {
        // Increment counter
        enemiesKilled++;
        
        // Update UI
        UpdateEnemyCountText();
        
        // Check win condition
        if (enemiesKilled >= enemiesRemainingToWin)
        {
            WinGame();
        }
    }
    
    void UpdateEnemyCountText()
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = $"Enemies: {enemiesKilled} / {enemiesRemainingToWin}";
        }
    }
    
    void WinGame()
    {
        // Show win panel
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        
        // Pause the game
        Time.timeScale = 0f;
        
        // Enable cursor for menu (if in editor or standalone)
        #if UNITY_EDITOR || UNITY_STANDALONE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        #endif
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        // Set time scale
        Time.timeScale = isPaused ? 0f : 1f;
        
        // Toggle pause menu
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
        }
        
        // Toggle cursor (if in editor or standalone)
        #if UNITY_EDITOR || UNITY_STANDALONE
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
        #endif
    }
    
    public void RestartGame()
    {
        // Unpause
        Time.timeScale = 1f;
        
        // Reset enemy count
        enemiesKilled = 0;
        
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void LoadMainMenu()
    {
        // Unpause
        Time.timeScale = 1f;
        
        // Load main menu scene (assumed to be at index 0)
        SceneManager.LoadScene(0);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
