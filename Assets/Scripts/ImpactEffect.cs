using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactEffect : MonoBehaviour, IPooledObject
{
    [Header("Settings")]
    public float lifetime = 2f;
    public ParticleSystem impactParticle;
    public AudioClip[] impactSounds;
    public float decalSize = 0.5f;
    public bool createDecal = true;

    private float timer;
    private AudioSource audioSource;

    void Awake()
    {
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 20f;
        }
    }

    void Update()
    {
        // Count down lifetime
        timer -= Time.deltaTime;

        // Deactivate when timer expires
        if (timer <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

    public void OnObjectSpawn()
    {
        // Reset timer
        timer = lifetime;

        // Random size variation
        float size = Random.Range(0.8f, 1.2f) * decalSize;
        transform.localScale = Vector3.one * size;

        // Random rotation around impact normal
        transform.Rotate(Vector3.forward, Random.Range(0f, 360f));

        // Play particle effect
        if (impactParticle != null)
        {
            impactParticle.Clear();
            impactParticle.Play();
        }

        // Play random impact sound
        if (impactSounds != null && impactSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = impactSounds[Random.Range(0, impactSounds.Length)];
            if (clip != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(clip);
            }
        }
    }

    // Method to set the impact type based on surface
    public void SetImpactType(string surfaceTag)
    {
        // This can be expanded to have different effects based on surface type
        switch (surfaceTag)
        {
            case "Metal":
                // Adjust particles or sounds for metal impact
                break;
            case "Wood":
                // Adjust particles or sounds for wood impact
                break;
            case "Concrete":
                // Adjust particles or sounds for concrete impact
                break;
            // Add more surface types as needed
        }
    }
}
