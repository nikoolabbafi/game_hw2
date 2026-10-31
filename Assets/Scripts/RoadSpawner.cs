using UnityEngine;

public class RoadSpawner : MonoBehaviour
{
    [Header("Road Prefab")]
    [SerializeField] private GameObject roadSectionPrefab;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Road Settings")]
    [SerializeField] private int poolSize = 6;
    [SerializeField] private float sectionLength = 20f;
    [SerializeField] private float firstSectionCenterZ = 0f;

    [Header("Recycling")]
    [SerializeField] private float recycleBehindDistance = 15f;

    private Transform[] roadSections;

    private int recycleIndex;
    private float lastSectionCenterZ;

    private void Start()
    {
        if (roadSectionPrefab == null || player == null)
        {
            Debug.LogError(
                "RoadSpawner: Assign the road prefab and player."
            );

            enabled = false;
            return;
        }

        if (poolSize < 2 || sectionLength <= 0f)
        {
            Debug.LogError(
                "RoadSpawner: Pool size must be at least 2 " +
                "and section length must be positive."
            );

            enabled = false;
            return;
        }

        roadSections = new Transform[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            float z = firstSectionCenterZ + i * sectionLength;

            GameObject section = Instantiate(
                roadSectionPrefab,
                new Vector3(0f, 0f, z),
                Quaternion.identity,
                transform
            );

            section.name = "RoadSection_" + i;

            roadSections[i] = section.transform;
        }

        lastSectionCenterZ =
            firstSectionCenterZ + (poolSize - 1) * sectionLength;

        recycleIndex = 0;
    }

    private void Update()
    {
        if (roadSections == null || player == null)
            return;

        while (
            player.position.z >
            roadSections[recycleIndex].position.z
            + sectionLength * 0.5f
            + recycleBehindDistance
        )
        {
            Transform section = roadSections[recycleIndex];

            lastSectionCenterZ += sectionLength;

            section.position = new Vector3(
                0f,
                0f,
                lastSectionCenterZ
            );

            recycleIndex = (recycleIndex + 1) % poolSize;
        }
    }
}