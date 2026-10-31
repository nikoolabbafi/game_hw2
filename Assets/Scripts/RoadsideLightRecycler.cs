using UnityEngine;

[DisallowMultipleComponent]
public sealed class RoadsideLightRecycler : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] lampPairs = new Transform[0];
    [SerializeField] private Light[] streetLights = new Light[0];
    [SerializeField] private float spacing = 24f;
    [SerializeField] private float distanceBehind = 24f;

    private void OnEnable()
    {
        RefreshLights();
    }

    private void LateUpdate()
    {
        RefreshLights();
    }

    private void RefreshLights()
    {
        if (player == null || spacing <= 0f || lampPairs == null)
            return;

        int firstSection = Mathf.FloorToInt((player.position.z - distanceBehind) / spacing);
        int nearestSection = Mathf.FloorToInt(player.position.z / spacing);
        for (int i = 0; i < lampPairs.Length; i++)
        {
            if (lampPairs[i] == null) continue;
            int section = firstSection + i;
            Vector3 position = lampPairs[i].position;
            position.z = section * spacing;
            lampPairs[i].position = position;

            // we use just 4 lamp
            bool illuminate = section == nearestSection || section == nearestSection + 1;
            for (int side = 0; side < 2; side++)
            {
                int lightIndex = i * 2 + side;
                if (lightIndex < streetLights.Length && streetLights[lightIndex] != null)
                    streetLights[lightIndex].enabled = illuminate;
            }
        }
    }
}
