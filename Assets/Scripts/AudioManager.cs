using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AudioManager handles all game audio including music and sound effects
/// </summary>
public class AudioManager : MonoBehaviour
{
    // Singleton instance
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;
    
    [Header("Music Tracks")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip gameOverMusic;
    
    [Header("Settings")]
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float ambientVolume = 0.5f;
    [SerializeField] private float musicFadeDuration = 1f;
    
    // Sound effect pool for performance
    private Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private int poolSize = 10;
    
    private void Awake()
    {
        // Singleton setup
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
        
        InitializeAudioSources();
        InitializeSFXPool();
        LoadVolumeSettings();
    }
    
    /// <summary>
    /// Initializes audio sources if not assigned
    /// </summary>
    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        if (ambientSource == null)
        {
            GameObject ambientObj = new GameObject("AmbientSource");
            ambientObj.transform.SetParent(transform);
            ambientSource = ambientObj.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
        }
    }
    
    /// <summary>
    /// Initializes the sound effect pool
    /// </summary>
    private void InitializeSFXPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject sfxObj = new GameObject($"SFXPool_{i}");
            sfxObj.transform.SetParent(transform);
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxPool.Add(source);
        }
    }
    
    /// <summary>
    /// Plays a music track
    /// </summary>
    public void PlayMusic(MusicType type)
    {
        AudioClip clip = GetMusicClip(type);
        if (clip != null)
        {
            StartCoroutine(CrossfadeMusic(clip));
        }
    }
    
    /// <summary>
    /// Gets the appropriate music clip
    /// </summary>
    private AudioClip GetMusicClip(MusicType type)
    {
        switch (type)
        {
            case MusicType.Menu: return menuMusic;
            case MusicType.Gameplay: return gameplayMusic;
            case MusicType.Boss: return bossMusic;
            case MusicType.Victory: return victoryMusic;
            case MusicType.GameOver: return gameOverMusic;
            default: return null;
        }
    }
    
    /// <summary>
    /// Crossfades between music tracks
    /// </summary>
    private System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        float startVolume = musicSource.volume;
        
        // Fade out current music
        if (musicSource.isPlaying)
        {
            float elapsed = 0f;
            while (elapsed < musicFadeDuration / 2)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (musicFadeDuration / 2));
                yield return null;
            }
        }
        
        // Switch to new music
        musicSource.clip = newClip;
        musicSource.Play();
        
        // Fade in new music
        float elapsed2 = 0f;
        while (elapsed2 < musicFadeDuration / 2)
        {
            elapsed2 += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed2 / (musicFadeDuration / 2));
            yield return null;
        }
        
        musicSource.volume = musicVolume;
    }
    
    /// <summary>
    /// Plays a sound effect
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableSFXSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = sfxVolume * volumeScale;
            source.Play();
        }
    }
    
    /// <summary>
    /// Plays a sound effect at a specific position
    /// </summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * volumeScale);
    }
    
    /// <summary>
    /// Gets an available audio source from the pool
    /// </summary>
    private AudioSource GetAvailableSFXSource()
    {
        foreach (AudioSource source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        // All sources busy, use the first one
        return sfxPool[0];
    }
    
    /// <summary>
    /// Sets the music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        SaveVolumeSettings();
    }
    
    /// <summary>
    /// Sets the SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveVolumeSettings();
    }
    
    /// <summary>
    /// Sets the ambient volume
    /// </summary>
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        ambientSource.volume = ambientVolume;
        SaveVolumeSettings();
    }
    
    /// <summary>
    /// Pauses all audio
    /// </summary>
    public void PauseAll()
    {
        musicSource.Pause();
        ambientSource.Pause();
    }
    
    /// <summary>
    /// Resumes all audio
    /// </summary>
    public void ResumeAll()
    {
        musicSource.UnPause();
        ambientSource.UnPause();
    }
    
    /// <summary>
    /// Stops all audio
    /// </summary>
    public void StopAll()
    {
        musicSource.Stop();
        ambientSource.Stop();
        foreach (AudioSource source in sfxPool)
        {
            source.Stop();
        }
    }
    
    /// <summary>
    /// Saves volume settings to PlayerPrefs
    /// </summary>
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("AmbientVolume", ambientVolume);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Loads volume settings from PlayerPrefs
    /// </summary>
    private void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.5f);
        
        musicSource.volume = musicVolume;
        ambientSource.volume = ambientVolume;
    }
    
    /// <summary>
    /// Gets current music volume
    /// </summary>
    public float GetMusicVolume() => musicVolume;
    
    /// <summary>
    /// Gets current SFX volume
    /// </summary>
    public float GetSFXVolume() => sfxVolume;
    
    /// <summary>
    /// Gets current ambient volume
    /// </summary>
    public float GetAmbientVolume() => ambientVolume;
}

/// <summary>
/// Music type enumeration
/// </summary>
public enum MusicType
{
    Menu,
    Gameplay,
    Boss,
    Victory,
    GameOver
}
