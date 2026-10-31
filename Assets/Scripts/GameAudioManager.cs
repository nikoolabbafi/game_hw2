using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip engineLoop;
    [SerializeField] private AudioClip collisionSound;
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip fuelSound;

    [Header("Scene Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource engineSource;
    [SerializeField] private AudioSource pickupSource;
    [SerializeField] private AudioSource collisionSource;
    [SerializeField] private PlayerLaneController player;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.28f;
    [SerializeField, Range(0f, 1f)] private float engineVolume = 0.16f;
    [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.8f;

    [Header("Engine Pitch")]
    [SerializeField] private float lowPitchSpeed = 8f;
    [SerializeField] private float highPitchSpeed = 20f;

    private bool audioPaused;
    private bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeSceneAudio()
    {
        GameAudioManager audio = Object.FindAnyObjectByType<GameAudioManager>();
        if (audio != null)
            audio.InitializeAudio();
    }

    private void Awake()
    {
        InitializeAudio();
    }

    private void InitializeAudio()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Only one GameAudioManager should be enabled in the scene.", this);
            enabled = false;
            return;
        }

        Instance = this;
        initialized = false;
        if (backgroundMusic == null || engineLoop == null || collisionSound == null ||
            coinSound == null || fuelSound == null || player == null || musicSource == null ||
            engineSource == null || pickupSource == null || collisionSource == null)
        {
            Debug.LogError("AudioManager: Assign all clips, sources and Player in the Inspector.", this);
            return;
        }

        ConfigureSource(musicSource, backgroundMusic, true, 128);
        ConfigureSource(engineSource, engineLoop, true, 160);
        ConfigureSource(pickupSource, null, false, 64);
        ConfigureSource(collisionSource, null, false, 48);
        musicSource.volume = 0f;
        engineSource.volume = 0f;
        engineSource.pitch = 0.9f;
        pickupSource.volume = effectsVolume * masterVolume;
        collisionSource.volume = effectsVolume * masterVolume;
        audioPaused = false;
        initialized = true;
        musicSource.Play();
    }

    private static void ConfigureSource(AudioSource source, AudioClip clip, bool loop, int priority)
    {
        source.Stop();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;
        source.priority = priority;
        source.clip = clip;
    }

    private void LateUpdate()
    {
        if (!initialized)
            return;

        GameManager game = GameManager.Instance;
        bool paused = game != null && game.IsPaused;
        SetPaused(paused);
        if (paused)
            return;

        bool gameOver = game != null && game.IsGameOver;
        bool driving = game != null && !gameOver && Time.timeScale > 0f && player != null;
        float musicTarget = musicVolume * masterVolume * (gameOver ? 0.35f : driving ? 1f : 0.7f);
        musicSource.volume = Mathf.MoveTowards(musicSource.volume, musicTarget, Time.unscaledDeltaTime);

        if (driving)
        {
            float speedRatio = Mathf.InverseLerp(lowPitchSpeed, highPitchSpeed, player.ForwardSpeed);
            float targetPitch = Mathf.Lerp(0.9f, 1.65f, speedRatio);
            engineSource.pitch = Mathf.MoveTowards(engineSource.pitch, targetPitch, 1.5f * Time.unscaledDeltaTime);
            if (!engineSource.isPlaying)
                engineSource.Play();
        }
        else
        {
            engineSource.Stop();
        }

        float engineTarget = driving ? engineVolume * masterVolume : 0f;
        engineSource.volume = Mathf.MoveTowards(engineSource.volume, engineTarget, 1.5f * Time.unscaledDeltaTime);
        pickupSource.volume = effectsVolume * masterVolume;
        collisionSource.volume = effectsVolume * masterVolume;
    }

    private void SetPaused(bool paused)
    {
        if (paused == audioPaused)
            return;

        audioPaused = paused;
        SetSourcePaused(musicSource, paused);
        SetSourcePaused(engineSource, paused);
        SetSourcePaused(pickupSource, paused);
        SetSourcePaused(collisionSource, paused);
    }

    private static void SetSourcePaused(AudioSource source, bool paused)
    {
        if (paused) source.Pause();
        else source.UnPause();
    }

    private void PlayEffect(AudioSource source, AudioClip clip, float volumeScale)
    {
        GameManager game = GameManager.Instance;
        if (!initialized || (game != null && game.IsPaused) || Time.timeScale <= 0f)
            return;

        source.volume = effectsVolume * masterVolume;
        source.PlayOneShot(clip, volumeScale);
    }

    public static void PlayCoinPickup()
    {
        if (Instance != null)
            Instance.PlayEffect(Instance.pickupSource, Instance.coinSound, 0.75f);
    }

    public static void PlayFuelPickup()
    {
        if (Instance != null)
            Instance.PlayEffect(Instance.pickupSource, Instance.fuelSound, 0.8f);
    }

    public static void PlayCollision()
    {
        if (Instance != null)
            Instance.PlayEffect(Instance.collisionSource, Instance.collisionSound, 1f);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
