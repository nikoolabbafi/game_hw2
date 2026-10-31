using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageFeedback : MonoBehaviour
{
    [Header("Feedback Settings")]
    [SerializeField] private Color damageColor = new Color(1f, 0.32f, 0.02f, 1f);
    [SerializeField, Range(0f, 5f)] private float damageEmissionIntensity = 2.5f;
    [SerializeField] private float blinkInterval = 0.12f;
    [SerializeField] private int blinkCount = 2;

    private readonly List<Material> materials = new List<Material>();
    private readonly List<Color> originalColors = new List<Color>();
    private readonly List<Color> originalEmissionColors = new List<Color>();
    private readonly List<bool> originalEmissionKeywords = new List<bool>();
    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            
            if (renderer is ParticleSystemRenderer || renderer.GetComponentInParent<DynamicNeonLighting>() != null)
                continue;

            foreach (Material material in renderer.materials)
            {
                string colorProperty = material.HasProperty("_BaseColor") ? "_BaseColor" :
                    material.HasProperty("_Color") ? "_Color" : null;
                if (colorProperty == null) continue;
                materials.Add(material);
                originalColors.Add(material.GetColor(colorProperty));
                originalEmissionColors.Add(material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black);
                originalEmissionKeywords.Add(material.IsKeywordEnabled("_EMISSION"));
            }
        }
    }

    public void PlayDamageFeedback()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            RestoreOriginalColors();
        }
        feedbackCoroutine = StartCoroutine(DamageFeedbackRoutine());
    }

    private IEnumerator DamageFeedbackRoutine()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            SetDamageColor();
     
            yield return new WaitForSecondsRealtime(blinkInterval);
            RestoreOriginalColors();
            yield return new WaitForSecondsRealtime(blinkInterval);
        }
        feedbackCoroutine = null;
    }

    private void SetDamageColor()
    {
        foreach (Material material in materials)
        {
            if (material == null) continue;
            material.SetColor(material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color", damageColor);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", damageColor * damageEmissionIntensity);
            }
        }
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < materials.Count; i++)
        {
            Material material = materials[i];
            if (material == null) continue;
            material.SetColor(material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color", originalColors[i]);
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", originalEmissionColors[i]);
                if (originalEmissionKeywords[i]) material.EnableKeyword("_EMISSION");
                else material.DisableKeyword("_EMISSION");
            }
        }
    }

    private void OnDisable()
    {
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = null;
        RestoreOriginalColors();
    }
}
