using UnityEngine;

public class MusicController : MonoBehaviour
{
    public static MusicController Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip defaultTrack;
    [Range(0f, 1f)][SerializeField] private float volume = 0.8f;
    [SerializeField] private bool playOnStart = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!source) source = GetComponent<AudioSource>();
        if (!source) source = gameObject.AddComponent<AudioSource>();

        source.loop = true;
        source.playOnAwake = false;
        source.volume = volume;
    }

    private void Start()
    {
        if (defaultTrack) source.clip = defaultTrack;
        if (playOnStart && source.clip) source.Play();
    }

    public void Play(AudioClip clip = null)
    {
        if (clip && source.clip != clip) source.clip = clip;
        if (!source.clip) { Debug.LogWarning("[MusicController] No clip to play"); return; }

        if (!source.isPlaying) source.Play();
    }

    public void Stop()
    {
        if (source.isPlaying) source.Stop();
    }

    public void Pause(bool paused)
    {
        if (paused) source.Pause();
        else source.UnPause();
    }

    public void SetVolume(float v01)
    {
        volume = Mathf.Clamp01(v01);
        source.volume = volume;
    }

    public bool IsPlaying => source && source.isPlaying;
}
