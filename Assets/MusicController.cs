using UnityEngine;

public class MusicController : MonoBehaviour
{
    public static MusicController Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip defaultTrack;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.8f;
    [SerializeField] private bool playOnStart = true;

    [Header("SFX (Cheer / Boo)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip[] cheerClips;
    [SerializeField] private AudioClip[] booClips;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!musicSource) musicSource = GetComponent<AudioSource>();
        if (!musicSource) musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        if (!sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        sfxSource.volume = sfxVolume;
    }

    private void Start()
    {
        if (defaultTrack) musicSource.clip = defaultTrack;
        if (playOnStart && musicSource.clip) musicSource.Play();
    }

    // ---- Music ----
    public void Play(AudioClip clip = null)
    {
        if (clip && musicSource.clip != clip) musicSource.clip = clip;
        if (!musicSource.clip) { Debug.LogWarning("[MusicController] No music clip to play"); return; }
        if (!musicSource.isPlaying) musicSource.Play();
    }

    public void Stop() { if (musicSource.isPlaying) musicSource.Stop(); }
    public void Pause(bool paused) { if (paused) musicSource.Pause(); else musicSource.UnPause(); }
    public void SetMusicVolume(float v01) { musicVolume = Mathf.Clamp01(v01); musicSource.volume = musicVolume; }

    // ---- SFX ----
    public void PlayCheer() => PlayRandomOneShot(cheerClips, "cheer");
    public void PlayBoo()   => PlayRandomOneShot(booClips, "boo");
    public void SetSfxVolume(float v01) { sfxVolume = Mathf.Clamp01(v01); sfxSource.volume = sfxVolume; }

    private void PlayRandomOneShot(AudioClip[] clips, string label)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"[MusicController] No {label} clips assigned");
            return;
        }

        var clip = clips[Random.Range(0, clips.Length)];
        if (!clip)
        {
            Debug.LogWarning($"[MusicController] {label} clip was null");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }
}
