using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public Button playButton;
    public Button settingsButton;
    public Button exitButton;
    
    [Header("Settings Menu")]
    public GameObject settingsPanel;
    public Slider sensitivitySlider;
    public Slider volumeSlider;
    public TMP_Dropdown qualityDropdown;
    public Button backButton;
    
    [Header("Game Settings")]
    public float defaultSensitivity = 3f;
    public float defaultVolume = 0.8f;
    
    void Start()
    {
        // Set up main menu
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        
        // Hide settings panel
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Set up button events
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }
        
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettings);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(QuitGame);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(CloseSettings);
        }
        
        // Load saved settings
        LoadSettings();
        
        // Set up settings controls
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        }
        
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        
        if (qualityDropdown != null)
        {
            // Set up quality levels
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
    }
    
    void StartGame()
    {
        // Load the game scene (assuming it's at index 1)
        SceneManager.LoadScene(1);
    }
    
    void OpenSettings()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }
    
    void CloseSettings()
    {
        // Save settings when closing
        SaveSettings();
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
    
    void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    void LoadSettings()
    {
        // Load sensitivity
        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = sensitivity;
        }
        
        // Load volume
        float volume = PlayerPrefs.GetFloat("Volume", defaultVolume);
        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
        }
        AudioListener.volume = volume;
        
        // Load quality
        int quality = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        if (qualityDropdown != null)
        {
            qualityDropdown.value = quality;
        }
        QualitySettings.SetQualityLevel(quality);
    }
    
    void SaveSettings()
    {
        // Save current settings
        if (sensitivitySlider != null)
        {
            PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
        }
        
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        }
        
        if (qualityDropdown != null)
        {
            PlayerPrefs.SetInt("Quality", qualityDropdown.value);
        }
        
        PlayerPrefs.Save();
    }
    
    void SetSensitivity(float value)
    {
        // Store sensitivity value (used by player controller)
        PlayerPrefs.SetFloat("Sensitivity", value);
    }
    
    void SetVolume(float value)
    {
        // Set audio listener volume (affects all audio)
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Volume", value);
    }
    
    void SetQuality(int index)
    {
        // Set quality level
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("Quality", index);
    }
}
