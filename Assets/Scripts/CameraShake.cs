using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Shake Settings")]
    public float maxShakeAmount = 0.5f;
    public float maxShakeTime = 1.0f;
    public float shakeFrequency = 25f;
    
    [Header("Shake Reduction")]
    public AnimationCurve shakeFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    private float shakeTimer;
    private float shakeAmount;
    private Vector3 originalPosition;
    private bool isShaking = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    
    void Start()
    {
        // Store original position
        originalPosition = transform.localPosition;
    }
    
    void Update()
    {
        // Skip if not shaking
        if (!isShaking) return;
        
        // Count down timer
        shakeTimer -= Time.deltaTime;
        
        if (shakeTimer > 0)
        {
            // Calculate shake reduction over time
            float progress = 1f - (shakeTimer / maxShakeTime);
            float currentShakeAmount = shakeAmount * shakeFalloff.Evaluate(progress);
            
            // Generate perlin noise for more natural shake
            float offsetX = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0) * 2 - 1) * currentShakeAmount;
            float offsetY = (Mathf.PerlinNoise(0, Time.time * shakeFrequency) * 2 - 1) * currentShakeAmount;
            
            // Apply shake
            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);
        }
        else
        {
            // Reset position and stop shaking
            transform.localPosition = originalPosition;
            isShaking = false;
        }
    }
    
    /// <summary>
    /// Start a camera shake effect
    /// </summary>
    /// <param name="intensity">Intensity from 0 to 1, will be multiplied by maxShakeAmount</param>
    /// <param name="duration">Duration in seconds, clamped by maxShakeTime</param>
    public void ShakeCamera(float intensity, float duration)
    {
        // Skip if already shaking more intensely
        if (isShaking && intensity < shakeAmount) return;
        
        // Store original position if not already shaking
        if (!isShaking)
        {
            originalPosition = transform.localPosition;
        }
        
        // Calculate shake amount based on intensity
        shakeAmount = Mathf.Clamp01(intensity) * maxShakeAmount;
        
        // Set timer
        shakeTimer = Mathf.Clamp(duration, 0, maxShakeTime);
        
        // Start shaking
        isShaking = true;
    }
    
    /// <summary>
    /// Stops any current camera shake immediately
    /// </summary>
    public void StopShake()
    {
        isShaking = false;
        transform.localPosition = originalPosition;
    }
}
