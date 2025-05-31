using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;

        [HideInInspector]
        public AudioSource source;
    }

    [Header("Sound Effects")]
    public SoundEffect[] soundEffects;

    [Header("Background Music")]
    public AudioClip[] backgroundMusic;
    public float musicVolume = 0.5f;
    public bool playMusicOnStart = true;
    public bool shuffleMusic = true;

    private AudioSource musicSource;
    private int currentMusicIndex = 0;

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

        // Set up music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = false; // We'll handle looping manually for playlist functionality
        musicSource.volume = musicVolume;

        // Set up sound effect sources
        foreach (SoundEffect sound in soundEffects)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
        }
    }

    void Start()
    {
        // Start playing background music if enabled
        if (playMusicOnStart && backgroundMusic.Length > 0)
        {
            if (shuffleMusic)
            {
                ShuffleMusic();
            }
            PlayNextMusic();
        }
    }

    void Update()
    {
        // Check if music has finished and play the next track
        if (musicSource != null && backgroundMusic.Length > 0 && !musicSource.isPlaying)
        {
            PlayNextMusic();
        }
    }

    void PlayNextMusic()
    {
        if (backgroundMusic.Length == 0) return;

        // Set the next track
        musicSource.clip = backgroundMusic[currentMusicIndex];
        musicSource.volume = musicVolume;
        musicSource.Play();

        // Increment index for next track
        currentMusicIndex = (currentMusicIndex + 1) % backgroundMusic.Length;
    }

    void ShuffleMusic()
    {
        // Simple Fisher-Yates shuffle
        for (int i = 0; i < backgroundMusic.Length; i++)
        {
            AudioClip temp = backgroundMusic[i];
            int randomIndex = Random.Range(i, backgroundMusic.Length);
            backgroundMusic[i] = backgroundMusic[randomIndex];
            backgroundMusic[randomIndex] = temp;
        }
    }

    public void PlaySound(string name)
    {
        // Find the sound by name
        SoundEffect sound = System.Array.Find(soundEffects, s => s.name == name);
        
        if (sound == null)
        {
            Debug.LogWarning("Sound effect " + name + " not found!");
            return;
        }

        // Play the sound
        sound.source.Play();
    }

    public void StopSound(string name)
    {
        // Find the sound by name
        SoundEffect sound = System.Array.Find(soundEffects, s => s.name == name);
        
        if (sound == null)
        {
            Debug.LogWarning("Sound effect " + name + " not found!");
            return;
        }

        // Stop the sound
        sound.source.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSoundVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        foreach (SoundEffect sound in soundEffects)
        {
            if (sound.source != null)
            {
                sound.source.volume = volume * sound.volume; // Apply master volume but keep relative volumes
            }
        }
    }

    public void PauseAllSounds(bool pause)
    {
        // Pause/unpause music
        if (musicSource != null)
        {
            if (pause)
            {
                if (musicSource.isPlaying)
                {
                    musicSource.Pause();
                }
            }
            else
            {
                if (!musicSource.isPlaying)
                {
                    musicSource.UnPause();
                }
            }
        }

        // Pause/unpause sound effects
        foreach (SoundEffect sound in soundEffects)
        {
            if (sound.source != null)
            {
                if (pause)
                {
                    if (sound.source.isPlaying)
                    {
                        sound.source.Pause();
                    }
                }
                else
                {
                    if (!sound.source.isPlaying && sound.loop)
                    {
                        sound.source.UnPause();
                    }
                }
            }
        }
    }
}
