using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public sealed class GameVfxManager : MonoBehaviour
{
    public static GameVfxManager Instance { get; private set; }

    [Header("Scene References")]
    [SerializeField] private PlayerLaneController player;
    [SerializeField] private Material glowMaterial;
    [SerializeField] private Material dissolveMaterial;
    [Header("Pool Sizes")]
    [SerializeField, Min(1)] private int collisionPoolSize = 6;
    [SerializeField, Min(1)] private int pickupPoolSize = 6;
    [SerializeField, Min(1)] private int dissolvePoolSize = 6;
    [SerializeField, Range(0.1f, 1f)] private float dissolveDuration = 0.38f;
    [SerializeField] private bool celebrateNewRecord = true;

    [Header("Sky Fireworks")]
    [SerializeField] private bool enableSkyFireworks = true;
    [SerializeField, Min(1f)] private float skyFireworkInterval = 4f;

    private ParticleSystem[] collisions;
    private ParticleSystem[] coins;
    private ParticleSystem[] fuel;
    private ParticleSystem[] fireworks;
    private ParticleSystem[] skyBursts;
    private ParticleSystem[] skyRockets;
    private const float RocketFlightTime = 0.65f;
    private float skyTimer;
    private float rocketRemaining;
    private bool rocketPending;
    private int skySide = -1;
    private Vector3 rocketTarget;
    private Color rocketColor;
    private static readonly Color[] SkyColors =
    {
        new Color(0.05f, 0.75f, 1f),
        new Color(1f, 0.15f, 0.65f),
        new Color(1f, 0.7f, 0.1f),
        new Color(0.6f, 0.2f, 1f),
        new Color(0.1f, 1f, 0.55f)
    };
    private PickupDissolveVisual[] dissolves;
    private bool initialized;
    private bool frozen;
    private bool recordCelebrated;
    private int startingHighScore;
    private int nextDissolve;
    private static readonly Color CoinColor = new Color(1f, 0.65f, 0.08f);
    private static readonly Color FuelColor = new Color(0.1f, 1f, 0.65f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeSceneVfx()
    {
        GameVfxManager manager = Object.FindAnyObjectByType<GameVfxManager>();
        if (manager == null) return;
        Instance = manager;
        manager.Initialize();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Only one GameVfxManager should be enabled in the scene.", this);
            enabled = false;
            return;
        }
        Instance = this;
        Initialize();
    }

    private void Initialize()
    {
        if (initialized && collisions != null && collisions[0] != null) return;
        if (glowMaterial == null || dissolveMaterial == null || player == null)
        {
            Debug.LogError("VFXManager: Assign Player and both VFX materials in the Inspector.", this);
            return;
        }
        collisions = CreatePool("CollisionSparks", Mathf.Max(1, collisionPoolSize), new Color(1f, 0.6f, 0.12f), 0.42f, 2.8f, 0.025f, true);
        coins = CreatePool("CoinBurst", Mathf.Max(1, pickupPoolSize), CoinColor, 0.42f, 0.9f, 0.055f, false);
        fuel = CreatePool("FuelBurst", Mathf.Max(1, pickupPoolSize), FuelColor, 0.5f, 0.75f, 0.065f, false);
        fireworks = CreatePool("RecordFireworks", 2, Color.white, 1.1f, 3f, 0.07f, true);
        skyBursts = CreatePool("SkyFireworkBurst", 2, Color.white, 1.2f, 3.4f, 0.07f, true);
        skyRockets = CreatePool("SkyFireworkRocket", 1, Color.white, RocketFlightTime, 5f, 0.09f, true);
        var rocketMain = skyRockets[0].main;
        rocketMain.gravityModifier = 0f;
        dissolves = new PickupDissolveVisual[Mathf.Max(1, dissolvePoolSize)];
        for (int i = 0; i < dissolves.Length; i++)
        {
            GameObject child = new GameObject("PickupDissolve_" + i);
            child.transform.SetParent(transform, false);
            dissolves[i] = child.AddComponent<PickupDissolveVisual>();
            dissolves[i].Initialize(dissolveMaterial, 8, dissolveDuration);
        }
        initialized = true;
    }

    private void Start()
    {
        startingHighScore = GameManager.Instance != null ? GameManager.Instance.HighScore : 0;
        recordCelebrated = false;
        skyTimer = 1.5f;
        rocketPending = false;
    }

    private ParticleSystem[] CreatePool(string name, int count, Color color, float lifetime, float speed, float size, bool streaks)
    {
        ParticleSystem[] pool = new ParticleSystem[count];
        for (int i = 0; i < count; i++)
        {
            ParticleSystem system = VfxParticleFactory.Create(name + "_" + i, transform, glowMaterial, false, true);
            var main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.6f, lifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.5f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
            main.startColor = color;
            main.gravityModifier = streaks ? 0.35f : -0.03f;
            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f;
            var emission = system.emission;
            emission.enabled = false;
            var sizeOverLife = system.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
            if (streaks)
            {
                var renderer = system.GetComponent<ParticleSystemRenderer>();
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.lengthScale = 1.5f;
                renderer.velocityScale = 0.08f;
            }
            system.gameObject.SetActive(true);
            pool[i] = system;
        }
        return pool;
    }

    private void Update()
    {
        if (!initialized) return;
        GameManager game = GameManager.Instance;
        bool gameOver = game != null && game.IsGameOver;
        bool freeze = (game != null && game.IsPaused) || (Time.timeScale <= 0f && !gameOver);
        if (freeze != frozen)
        {
            SetPoolPaused(collisions, freeze);
            SetPoolPaused(coins, freeze);
            SetPoolPaused(fuel, freeze);
            SetPoolPaused(fireworks, freeze);
            SetPoolPaused(skyBursts, freeze);
            SetPoolPaused(skyRockets, freeze);
            frozen = freeze;
        }
        if (celebrateNewRecord && !recordCelebrated && startingHighScore > 0 && game != null &&
            !freeze && !gameOver && game.Score > startingHighScore)
        {
            recordCelebrated = true;
            Emit(fireworks, player.transform.position + new Vector3(-3.2f, 2.6f, 10f), 48, new Color(0.05f, 0.75f, 1f));
            Emit(fireworks, player.transform.position + new Vector3(3.2f, 3.2f, 12f), 48, new Color(1f, 0.15f, 0.65f));
        }
        UpdateSkyFireworks(game, freeze, gameOver);
    }

    private void UpdateSkyFireworks(GameManager game, bool freeze, bool gameOver)
    {
        if (freeze) return;

        if (rocketPending)
        {
            rocketRemaining -= gameOver ? Time.unscaledDeltaTime : Time.deltaTime;
            if (rocketRemaining <= 0f)
            {
                Emit(skyBursts, rocketTarget, 56, rocketColor);
                rocketPending = false;
            }
        }
        if (!enableSkyFireworks || game == null || gameOver || Time.timeScale <= 0f) return;

        skyTimer -= Time.deltaTime;
        if (skyTimer <= 0f && !rocketPending)
        {
            LaunchSkyRocket();
            skyTimer = Mathf.Max(1f, skyFireworkInterval);
        }
    }

    private void LaunchSkyRocket()
    {
        rocketColor = SkyColors[Random.Range(0, SkyColors.Length)];
        float rise = Random.Range(3f, 4f);
        
        Vector3 launchPosition = player.transform.position + new Vector3(
            skySide * Random.Range(3.2f, 4.2f), 0.6f,
            18f + player.ForwardSpeed * RocketFlightTime);
        rocketTarget = launchPosition + Vector3.up * rise;
        skySide = -skySide;

        ParticleSystem rocket = skyRockets[0];
        rocket.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        rocket.transform.position = launchPosition;
        var parameters = new ParticleSystem.EmitParams
        {
            velocity = Vector3.up * (rise / RocketFlightTime),
            startLifetime = RocketFlightTime,
            startSize = 0.09f,
            startColor = rocketColor
        };
        rocket.Play(false);
        rocket.Emit(parameters, 1);
        rocketRemaining = RocketFlightTime;
        rocketPending = true;
    }

    private static void SetPoolPaused(ParticleSystem[] pool, bool pause)
    {
        foreach (ParticleSystem system in pool)
        {
            if (pause && system.isPlaying) system.Pause(false);
            else if (!pause && system.isPaused) system.Play(false);
        }
    }

    private bool CanPlay()
    {
        GameManager game = GameManager.Instance;
        return initialized && Time.timeScale > 0f && (game == null || (!game.IsPaused && !game.IsGameOver));
    }

    private static void Emit(ParticleSystem[] pool, Vector3 position, int count, Color color)
    {
        ParticleSystem chosen = pool[0];
        foreach (ParticleSystem candidate in pool)
            if (!candidate.IsAlive(false)) { chosen = candidate; break; }
        chosen.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        chosen.transform.position = position;
        var main = chosen.main;
        main.startColor = color;
        chosen.Play(false);
        chosen.Emit(count);
    }

    private void PlayPickup(GameObject item, bool isFuel)
    {
        if (!CanPlay() || item == null) return;
        Color color = isFuel ? FuelColor : CoinColor;
        Emit(isFuel ? fuel : coins, item.transform.position, 18, color);
        PickupDissolveVisual ghost = dissolves[nextDissolve];
        foreach (PickupDissolveVisual candidate in dissolves)
            if (!candidate.IsPlaying) { ghost = candidate; break; }
        nextDissolve = (nextDissolve + 1) % dissolves.Length;

        ghost.Play(item, color);
    }

    public static void PlayCoinPickup(GameObject item)
    {
        if (Instance != null) Instance.PlayPickup(item, false);
    }

    public static void PlayFuelPickup(GameObject item)
    {
        if (Instance != null) Instance.PlayPickup(item, true);
    }

    public static void PlayCollision(Vector3 position)
    {
        if (Instance != null && Instance.CanPlay())
            Emit(Instance.collisions, position, 24, new Color(1f, 0.6f, 0.12f));
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

internal static class VfxParticleFactory
{
    public static ParticleSystem Create(string name, Transform parent, Material material, bool loop, bool unscaled)
    {
        GameObject child = new GameObject(name);
        child.SetActive(false);
        child.transform.SetParent(parent, false);
        ParticleSystem system = child.AddComponent<ParticleSystem>();
        system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = system.main;
        main.playOnAwake = false;
        main.loop = loop;
        main.duration = loop ? 1f : 0.1f;
        main.useUnscaledTime = unscaled;
        main.maxParticles = 96;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        var renderer = system.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        Gradient fade = new Gradient();
        fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.7f, 0.5f), new GradientAlphaKey(0f, 1f) });
        var colorOverLife = system.colorOverLifetime;
        colorOverLife.enabled = true;
        colorOverLife.color = fade;
        return system;
    }
}
