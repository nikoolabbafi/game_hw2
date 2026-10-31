using UnityEngine;

[DisallowMultipleComponent]
public sealed class CarExhaustVfx : MonoBehaviour
{
    [SerializeField] private PlayerLaneController player;
    [SerializeField] private Material smokeMaterial;
    [SerializeField] private Material flameMaterial;
    [SerializeField] private float minimumSpeed = 8f;
    [SerializeField] private float maximumSpeed = 20f;

    private ParticleSystem smoke;
    private ParticleSystem flame;
    private bool wasDriving;
    private float lastSpeed;

    private void Awake()
    {
        if (player == null || smokeMaterial == null || flameMaterial == null)
        {
            Debug.LogError("ExhaustVFX: Assign Player, Smoke Material and Flame Material.", this);
            enabled = false;
            return;
        }
        BuildEffects();
        lastSpeed = player.ForwardSpeed;
    }

    private void BuildEffects()
    {
        smoke = VfxParticleFactory.Create("ExhaustSmoke", transform, smokeMaterial, true, false);
        var main = smoke.main;
        main.maxParticles = 48;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.1f);
        main.startColor = new Color(0.35f, 0.4f, 0.48f, 0.22f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        var smokeShape = smoke.shape;
        smokeShape.enabled = true;
        smokeShape.shapeType = ParticleSystemShapeType.Cone;
        smokeShape.angle = 12f;
        smokeShape.radius = 0.025f;
        var velocity = smoke.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.y = 0.25f;
        var size = smoke.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.5f, 1f, 2.2f));
        var emission = smoke.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        smoke.gameObject.SetActive(true);

        flame = VfxParticleFactory.Create("ExhaustFlame", transform, flameMaterial, true, false);
        main = flame.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 32;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.65f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
        main.startColor = new Color(1f, 0.37f, 0.08f);
        var flameShape = flame.shape;
        flameShape.enabled = true;
        flameShape.shapeType = ParticleSystemShapeType.Cone;
        flameShape.angle = 8f;
        flameShape.radius = 0.015f;
        emission = flame.emission;
        emission.rateOverTime = 0f;
        flame.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (smoke == null || flame == null || player == null) return;
        GameManager game = GameManager.Instance;
        if (game != null && game.IsPaused) return;
        bool driving = game != null && !game.IsGameOver && Time.timeScale > 0f;
        if (!driving)
        {
            if (wasDriving)
            {
                smoke.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                flame.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            wasDriving = false;
            lastSpeed = player.ForwardSpeed;
            return;
        }

        float ratio = Mathf.InverseLerp(minimumSpeed, maximumSpeed, player.ForwardSpeed);
        var smokeEmission = smoke.emission;
        smokeEmission.rateOverTime = Mathf.Lerp(8f, 20f, ratio);
        var flameEmission = flame.emission;
        flameEmission.rateOverTime = Mathf.Lerp(4f, 14f, ratio);
        if (!wasDriving)
        {
            smoke.Play(false);
            flame.Play(false);
        }
        if (player.ForwardSpeed > lastSpeed + 0.01f)
            flame.Emit(8);
        lastSpeed = player.ForwardSpeed;
        wasDriving = true;
    }
}
