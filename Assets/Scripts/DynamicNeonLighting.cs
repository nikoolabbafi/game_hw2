using UnityEngine;

[DisallowMultipleComponent]
public sealed class DynamicNeonLighting : MonoBehaviour
{
    [SerializeField] private PlayerLaneController player;
    [SerializeField] private Light[] underglowLights = new Light[0];
    [SerializeField] private Renderer[] neonRenderers = new Renderer[0];
    [SerializeField] private Color neonColor = new Color(0.02f, 0.85f, 1f);
    [SerializeField] private float minimumSpeed = 8f;
    [SerializeField] private float maximumSpeed = 20f;
    [SerializeField] private float minimumIntensity = 1.5f;
    [SerializeField] private float maximumIntensity = 4.5f;
    [SerializeField] private float minimumEmission = 2f;
    [SerializeField] private float maximumEmission = 5.5f;
    [SerializeField] private float responseSpeed = 6f;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private MaterialPropertyBlock properties;
    private float speedRatio;

    private void OnEnable()
    {
        properties = new MaterialPropertyBlock();
        speedRatio = GetSpeedRatio();
        ApplyLighting();
    }

    private float GetSpeedRatio()
    {
        return player != null ? Mathf.InverseLerp(minimumSpeed, maximumSpeed, player.ForwardSpeed) : 0f;
    }

    private void LateUpdate()
    {
        
        speedRatio = Mathf.Lerp(speedRatio, GetSpeedRatio(), 1f - Mathf.Exp(-responseSpeed * Time.deltaTime));
        ApplyLighting();
    }

    private void ApplyLighting()
    {
        float intensity = Mathf.Lerp(minimumIntensity, maximumIntensity, speedRatio);
        foreach (Light light in underglowLights)
            if (light != null) light.intensity = intensity;

        properties.SetColor(EmissionColor, neonColor * Mathf.Lerp(minimumEmission, maximumEmission, speedRatio));
        foreach (Renderer neon in neonRenderers)
            if (neon != null) neon.SetPropertyBlock(properties);
    }
}
