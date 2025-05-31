using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceOptimizer : MonoBehaviour
{
    [Header("Quality Settings")]
    public bool dynamicResolution = true;
    public bool automaticQualityAdjustment = true;
    
    [Header("Frame Rate")]
    public int targetFrameRate = 60;
    public int lowEndTargetFrameRate = 30;
    
    [Header("LOD Settings")]
    public float lowEndLODBias = 1.5f;
    public float highEndLODBias = 1.0f;
    
    [Header("Draw Distance")]
    public float lowEndDrawDistance = 50f;
    public float highEndDrawDistance = 100f;
    
    [Header("Monitoring")]
    public float monitoringInterval = 2f;
    public int frameRateThreshold = 40;
    public int consecutiveLowFramesForAdjustment = 3;
    
    // Private variables
    private bool isLowEndDevice = false;
    private int consecutiveLowFrames = 0;
    private float frameRateCheckTimer;
    private int qualityLevel;
    
    void Start()
    {
        // Initialize
        frameRateCheckTimer = monitoringInterval;
        
        // Detect device capability
        DetectDeviceCapability();
        
        // Apply initial settings
        ApplyOptimalSettings();
    }
    
    void Update()
    {
        // Only monitor if automatic adjustment is enabled
        if (!automaticQualityAdjustment) return;
        
        // Monitor frame rate periodically
        frameRateCheckTimer -= Time.unscaledDeltaTime;
        if (frameRateCheckTimer <= 0f)
        {
            // Reset timer
            frameRateCheckTimer = monitoringInterval;
            
            // Check frame rate
            MonitorPerformance();
        }
    }
    
    void DetectDeviceCapability()
    {
        #if UNITY_ANDROID
        // Check system specs to determine if this is a low-end device
        bool lowCPU = SystemInfo.processorCount <= 4;
        bool lowMemory = SystemInfo.systemMemorySize <= 2048; // 2GB
        bool lowGPU = SystemInfo.graphicsMemorySize <= 1024; // 1GB
        
        // Set device tier based on specs
        isLowEndDevice = lowCPU || lowMemory || lowGPU;
        
        Debug.Log($"Device detected as: {(isLowEndDevice ? "Low-End" : "High-End")}");
        Debug.Log($"CPU cores: {SystemInfo.processorCount}, RAM: {SystemInfo.systemMemorySize}MB, GPU Memory: {SystemInfo.graphicsMemorySize}MB");
        #else
        // For non-Android platforms, assume high-end
        isLowEndDevice = false;
        #endif
    }
    
    void ApplyOptimalSettings()
    {
        // Set target frame rate
        Application.targetFrameRate = isLowEndDevice ? lowEndTargetFrameRate : targetFrameRate;
        
        // Set quality level
        int maxQualityLevel = QualitySettings.names.Length - 1;
        qualityLevel = isLowEndDevice ? Mathf.Min(1, maxQualityLevel) : Mathf.Min(3, maxQualityLevel);
        QualitySettings.SetQualityLevel(qualityLevel, true);
        
        // Apply LOD bias
        QualitySettings.lodBias = isLowEndDevice ? lowEndLODBias : highEndLODBias;
        
        // Apply camera draw distance
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.farClipPlane = isLowEndDevice ? lowEndDrawDistance : highEndDrawDistance;
        }
        
        // Apply dynamic resolution if supported
        #if UNITY_2019_3_OR_NEWER
        if (dynamicResolution)
        {
            if (isLowEndDevice)
            {
                // Lower min scale for low-end devices
                DynamicResolutionHandler.SetDynamicResScaler(
                    () => { return DynamicResolutionHandler.DefaultDynamicResMethod(); },
                    DynamicResolutionHandler.ScalerType.SCREEN_PERCENTAGE);
                
                DynamicResolutionHandler.minScreenFraction = 0.6f;
                DynamicResolutionHandler.maxScreenFraction = 1.0f;
            }
            else
            {
                // Higher min scale for high-end devices
                DynamicResolutionHandler.minScreenFraction = 0.8f;
                DynamicResolutionHandler.maxScreenFraction = 1.0f;
            }
            
            DynamicResolutionHandler.Enable(true);
        }
        #endif
        
        // Log applied settings
        Debug.Log($"Applied performance settings: Quality Level: {QualitySettings.GetQualityLevel()}, Target FPS: {Application.targetFrameRate}, LOD Bias: {QualitySettings.lodBias}");
    }
    
    void MonitorPerformance()
    {
        // Get current frame rate
        float currentFPS = 1.0f / Time.unscaledDeltaTime;
        
        // Check if performance is below threshold
        if (currentFPS < frameRateThreshold)
        {
            consecutiveLowFrames++;
            
            // If consistently low frames, lower quality
            if (consecutiveLowFrames >= consecutiveLowFramesForAdjustment)
            {
                LowerQuality();
                consecutiveLowFrames = 0;
            }
        }
        else
        {
            // Reset counter if frame rate is good
            consecutiveLowFrames = 0;
        }
    }
    
    void LowerQuality()
    {
        // If already at lowest quality, don't go further
        if (qualityLevel <= 0) return;
        
        // Lower quality level
        qualityLevel--;
        QualitySettings.SetQualityLevel(qualityLevel, true);
        
        // Lower draw distance further
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.farClipPlane = Mathf.Max(mainCamera.farClipPlane * 0.8f, 30f);
        }
        
        // Increase LOD bias (to use lower detail models)
        QualitySettings.lodBias = Mathf.Max(QualitySettings.lodBias * 1.2f, 2.0f);
        
        // Log quality adjustment
        Debug.Log($"Adjusted quality settings down to level: {qualityLevel}, LOD Bias: {QualitySettings.lodBias}, Draw Distance: {(mainCamera != null ? mainCamera.farClipPlane : 0)}");
    }
}
