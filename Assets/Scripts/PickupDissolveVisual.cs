using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class PickupDissolveVisual : MonoBehaviour
{
    private sealed class MeshPart
    {
        public MeshFilter Filter;
        public MeshRenderer Renderer;
        public MaterialPropertyBlock[] Properties;
    }

    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");
    private static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
    private static readonly int Dissolve = Shader.PropertyToID("_Dissolve");
    private readonly List<MeshFilter> sourceMeshes = new List<MeshFilter>();
    private MeshPart[] parts;
    private Material dissolveMaterial;
    private float duration;
    private float elapsed;
    private int usedParts;
    public bool IsPlaying { get; private set; }

    public void Initialize(Material material, int maximumMeshParts, float dissolveDuration)
    {
        dissolveMaterial = material;
        duration = Mathf.Max(0.05f, dissolveDuration);
        parts = new MeshPart[maximumMeshParts];
        for (int i = 0; i < parts.Length; i++)
        {
            GameObject child = new GameObject("DissolveMesh_" + i);
            child.transform.SetParent(transform, false);
            MeshRenderer renderer = child.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.enabled = false;
            parts[i] = new MeshPart { Filter = child.AddComponent<MeshFilter>(), Renderer = renderer };
        }
        gameObject.SetActive(false);
    }

    public void Play(GameObject item, Color edgeColor)
    {
        HideParts();
        usedParts = 0;
        sourceMeshes.Clear();
        item.GetComponentsInChildren(false, sourceMeshes);
        foreach (MeshFilter source in sourceMeshes)
        {
            if (usedParts >= parts.Length) break;
            MeshRenderer original;
            if (source.sharedMesh == null || !source.TryGetComponent(out original) || !original.enabled)
                continue;

            MeshPart part = parts[usedParts++];
            part.Filter.sharedMesh = source.sharedMesh;
            part.Filter.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            // Pool parents are created at identity; this preserves the original visual's world size.
            part.Filter.transform.localScale = source.transform.lossyScale;
            int materialCount = Mathf.Max(1, source.sharedMesh.subMeshCount);
            Material[] materials = new Material[materialCount];
            Material[] originals = original.sharedMaterials;
            if (part.Properties == null || part.Properties.Length != materialCount)
            {
                part.Properties = new MaterialPropertyBlock[materialCount];
                for (int m = 0; m < materialCount; m++)
                    part.Properties[m] = new MaterialPropertyBlock();
            }
            for (int m = 0; m < materialCount; m++)
            {
                materials[m] = dissolveMaterial;
                Material sourceMaterial = originals.Length > 0 ? originals[Mathf.Min(m, originals.Length - 1)] : null;
                MaterialPropertyBlock properties = part.Properties[m];
                properties.Clear();
                string textureName = sourceMaterial != null && sourceMaterial.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
                bool hasTexture = sourceMaterial != null && sourceMaterial.HasProperty(textureName);
                Texture texture = hasTexture ? sourceMaterial.GetTexture(textureName) : null;
                Vector2 scale = hasTexture ? sourceMaterial.GetTextureScale(textureName) : Vector2.one;
                Vector2 offset = hasTexture ? sourceMaterial.GetTextureOffset(textureName) : Vector2.zero;
                Color color = Color.white;
                if (sourceMaterial != null && sourceMaterial.HasProperty("_BaseColor")) color = sourceMaterial.GetColor("_BaseColor");
                else if (sourceMaterial != null && sourceMaterial.HasProperty("_Color")) color = sourceMaterial.GetColor("_Color");
                properties.SetTexture(BaseMap, texture != null ? texture : Texture2D.whiteTexture);
                properties.SetVector(BaseMapST, new Vector4(scale.x, scale.y, offset.x, offset.y));
                properties.SetColor(BaseColor, color);
                properties.SetColor(EdgeColor, edgeColor * 4f);
                properties.SetFloat(Dissolve, 0f);
            }
            part.Renderer.sharedMaterials = materials;
            for (int m = 0; m < materialCount; m++)
                part.Renderer.SetPropertyBlock(part.Properties[m], m);
            part.Renderer.enabled = true;
        }

        elapsed = 0f;
        IsPlaying = usedParts > 0;
        gameObject.SetActive(IsPlaying);
    }

    private void Update()
    {
        if (!IsPlaying) return;
        GameManager game = GameManager.Instance;
        if (game != null && game.IsPaused) return;
        bool gameOver = game != null && game.IsGameOver;
        if (Time.timeScale <= 0f && !gameOver) return;
        elapsed += gameOver ? Time.unscaledDeltaTime : Time.deltaTime;
        float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
        for (int i = 0; i < usedParts; i++)
            for (int m = 0; m < parts[i].Properties.Length; m++)
            {
                parts[i].Properties[m].SetFloat(Dissolve, progress);
                parts[i].Renderer.SetPropertyBlock(parts[i].Properties[m], m);
            }
        if (elapsed >= duration)
        {
            IsPlaying = false;
            HideParts();
            gameObject.SetActive(false);
        }
    }

    private void HideParts()
    {
        if (parts == null) return;
        foreach (MeshPart part in parts)
            if (part.Renderer != null) part.Renderer.enabled = false;
    }

    private void OnDisable()
    {
        IsPlaying = false;
        HideParts();
    }
}
