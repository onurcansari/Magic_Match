using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private const string MutedPrefKey = "AudioMuted";

    public static AudioManager Instance { get; private set; }

    public AudioSource musicSource;
    public AudioSource sfxSource;

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
