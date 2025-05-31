using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    [Header("UI Elements")]
    public GameObject loadingScreenPanel;
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI tipText;
    
    [Header("Loading Tips")]
    public string[] loadingTips;
    public float tipChangeInterval = 5f;
    
    // Private variables
    private bool isLoading = false;
    private float tipTimer;
    private int currentTipIndex = 0;

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
            return;
        }
        
        // Hide loading screen initially
        if (loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        // Skip if not loading or no tips
        if (!isLoading || loadingTips == null || loadingTips.Length == 0) return;
        
        // Update tip timer
        tipTimer -= Time.unscaledDeltaTime;
        
        if (tipTimer <= 0f)
        {
            // Reset timer
            tipTimer = tipChangeInterval;
            
            // Show next tip
            ShowNextTip();
        }
    }
    
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneAsync(sceneIndex));
    }
    
    IEnumerator LoadSceneAsync(int sceneIndex)
    {
        // Start loading
        isLoading = true;
        
        // Show loading screen
        if (loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(true);
        }
        
        // Reset progress
        if (progressBar != null)
        {
            progressBar.value = 0f;
        }
        
        // Start tip rotation
        currentTipIndex = 0;
        tipTimer = tipChangeInterval;
        ShowNextTip();
        
        // Start async load
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        
        // Don't let the scene activate until we allow it
        operation.allowSceneActivation = false;
        
        // Track loading progress
        while (!operation.isDone)
        {
            // Progress ranges from 0 to 0.9 during loading
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            // Update progress bar
            if (progressBar != null)
            {
                progressBar.value = progress;
            }
            
            // Update text
            if (progressText != null)
            {
                progressText.text = $"Loading: {Mathf.Floor(progress * 100)}%";
            }
            
            // If loading is almost complete
            if (operation.progress >= 0.9f)
            {
                // Allow scene activation when reaching 100% on the progress bar
                if (progressBar != null && progressBar.value >= 1.0f)
                {
                    operation.allowSceneActivation = true;
                }
                else
                {
                    // Gradually fill the last 10% of the progress bar
                    if (progressBar != null)
                    {
                        progressBar.value += Time.unscaledDeltaTime * 0.1f;
                    }
                }
            }
            
            yield return null;
        }
        
        // Hide loading screen
        if (loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(false);
        }
        
        // End loading
        isLoading = false;
    }
    
    void ShowNextTip()
    {
        // Skip if no tips or text component
        if (loadingTips == null || loadingTips.Length == 0 || tipText == null) return;
        
        // Update current tip index
        currentTipIndex = (currentTipIndex + 1) % loadingTips.Length;
        
        // Show tip
        tipText.text = loadingTips[currentTipIndex];
    }
}
