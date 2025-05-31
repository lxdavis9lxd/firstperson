using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlashEffect : MonoBehaviour, IPooledObject
{
    [Header("Settings")]
    public float lifetime = 0.05f;
    public float minSize = 0.5f;
    public float maxSize = 1.5f;
    public Light muzzleLight;
    public ParticleSystem muzzleParticle;

    private float timer;

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
        float size = Random.Range(minSize, maxSize);
        transform.localScale = Vector3.one * size;

        // Reset light
        if (muzzleLight != null)
        {
            muzzleLight.intensity = Random.Range(2f, 4f);
        }

        // Play particle
        if (muzzleParticle != null)
        {
            muzzleParticle.Clear();
            muzzleParticle.Play();
        }
    }
}
