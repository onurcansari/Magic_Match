using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private const string MutedPrefKey = "AudioMuted";
    private const string SfxResourcePath = "Audio/SFX/";
    private const string MusicResourcePath = "Audio/Music/";

    public static AudioManager Instance { get; private set; }

    public AudioSource musicSource;
    public AudioSource sfxSource;

    private readonly Dictionary<string, AudioClip> sfxCache = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, AudioClip> musicCache = new Dictionary<string, AudioClip>();
    private string currentMusicTrack;

    public bool IsMuted
    {
        get { return PlayerPrefs.GetInt(MutedPrefKey, 0) == 1; }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ApplyMuteState();

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    // Drop an audio file with this exact name (any extension Unity supports: wav/mp3/ogg)
    // into Assets/Resources/Audio/SFX/ or Assets/Resources/Audio/Music/ and it loads automatically -
    // no Inspector wiring needed. Missing clips are cached as null so we don't reload every frame.
    private string GetMusicTrackForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "MainScene":
            case "LevelSelect":
                return "menu_theme";
            case "BattlePath":
                return "battlepath_theme";
            default:
                return "gameplay_theme";
        }
    }

    private void PlayMusicForScene(string sceneName)
    {
        PlayMusicByName(GetMusicTrackForScene(sceneName));
    }

    public void PlayMusicByName(string trackName)
    {
        if (string.IsNullOrEmpty(trackName) || musicSource == null || trackName == currentMusicTrack)
        {
            return;
        }

        if (!musicCache.TryGetValue(trackName, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>(MusicResourcePath + trackName);
            musicCache[trackName] = clip;
        }

        if (clip == null)
        {
            return;
        }

        currentMusicTrack = trackName;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySfxByName(string sfxName)
    {
        if (string.IsNullOrEmpty(sfxName))
        {
            return;
        }

        if (!sfxCache.TryGetValue(sfxName, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>(SfxResourcePath + sfxName);
            sfxCache[sfxName] = clip;
        }

        PlaySfx(clip);
    }

    // Convenience static call used throughout the gameplay scripts; safely no-ops if no
    // AudioManager has been spawned yet (e.g. a level scene opened directly in the editor).
    public static void Play(string sfxName)
    {
        if (Instance != null)
        {
            Instance.PlaySfxByName(sfxName);
        }
    }

    public void SetMuted(bool muted)
    {
        PlayerPrefs.SetInt(MutedPrefKey, muted ? 1 : 0);
        ApplyMuteState();
    }

    public void ToggleMuted()
    {
        SetMuted(!IsMuted);
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || IsMuted || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    private void ApplyMuteState()
    {
        bool muted = IsMuted;

        if (musicSource != null)
        {
            musicSource.mute = muted;
        }

        if (sfxSource != null)
        {
            sfxSource.mute = muted;
        }
    }
}
