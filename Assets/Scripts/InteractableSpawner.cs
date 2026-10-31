
using UnityEngine;

public class InteractableSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InteractablePool pool;
    [SerializeField] private Transform player;

    [Header("Lane Settings")]
    [SerializeField] private float laneWidth = 1.8f;

    [Header("General Spawn Settings")]
    [SerializeField] private float spawnDistanceAhead = 35f;

    [Header("Collectible Spawn Settings")]
    [SerializeField] private float distanceBetweenCollectibleSpawns = 12f;

    [Range(0f, 1f)]
    [SerializeField] private float fuelCanChance = 0.15f;

    [Range(0f, 1f)]
    [SerializeField] private float coinChance = 0.50f;

    [Header("Obstacle Spawn Settings")]
    [SerializeField] private float obstacleSpawnDistance = 18f;

    [Range(0f, 1f)]
    [SerializeField] private float staticObstacleChance = 0.20f;

    [Range(0f, 1f)]
    [SerializeField] private float movingObstacleChance = 0.15f;

    [Header("Progressive Obstacle Difficulty")]
    [SerializeField] private float obstacleChanceIncrease = 0.05f;
    [SerializeField] private float obstacleDistanceDecrease = 2f;

    [Range(0f, 1f)]
    [SerializeField] private float maxStaticObstacleChance = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float maxMovingObstacleChance = 0.30f;

    [SerializeField] private float minObstacleSpawnDistance = 10f;

    [Header("Spawn Heights")]
    [SerializeField] private float collectibleHeight = 0.7f;
    [SerializeField] private float staticObstacleHeight = 0.15f;
    [SerializeField] private float movingObstacleHeight = 0f;

    private float nextCollectibleZ;
    private float nextObstacleZ;

    private void Start()
    {
        if (pool == null || player == null)
        {
            Debug.LogError(
                "InteractableSpawner: Pool or Player is not assigned."
            );

            enabled = false;
            return;
        }

        nextCollectibleZ =
            player.position.z + spawnDistanceAhead;

        nextObstacleZ =
            player.position.z +
            spawnDistanceAhead +
            obstacleSpawnDistance * 0.5f;
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
        {
            return;
        }

        float spawnLimitZ =
            player.position.z + spawnDistanceAhead;

  
        while (spawnLimitZ >= nextCollectibleZ)
        {
            SpawnCollectible(nextCollectibleZ);
            nextCollectibleZ += distanceBetweenCollectibleSpawns;
        }

        while (spawnLimitZ >= nextObstacleZ)
        {
            SpawnObstacle(nextObstacleZ);
            nextObstacleZ += obstacleSpawnDistance;
        }
    }

    private void SpawnCollectible(float spawnZ)
    {
        float randomValue = Random.value;

        GameObject item;
        float spawnHeight;

        if (randomValue < fuelCanChance)
        {
            item = pool.GetFuelCan();
            spawnHeight = collectibleHeight;
        }
        else if (randomValue < fuelCanChance + coinChance)
        {
            item = pool.GetCoin();
            spawnHeight = collectibleHeight;
        }
        else
        {
            return;
        }

        PlaceItem(item, spawnHeight, spawnZ);
    }

    private void SpawnObstacle(float spawnZ)
    {
        float randomValue = Random.value;

        GameObject item;
        float spawnHeight;

        if (randomValue < staticObstacleChance)
        {
            item = pool.GetStaticObstacle();
            spawnHeight = staticObstacleHeight;
        }
        else if (
            randomValue <
            staticObstacleChance + movingObstacleChance
        )
        {
            item = pool.GetMovingObstacle();
            spawnHeight = movingObstacleHeight;
        }
        else
        {
            
            return;
        }

        PlaceItem(item, spawnHeight, spawnZ);
    }

    private void PlaceItem(
        GameObject item,
        float spawnHeight,
        float spawnZ)
    {
        if (item == null)
            return;

        int randomLane = Random.Range(-1, 2);

        item.transform.position = new Vector3(
            randomLane * laneWidth,
            spawnHeight,
            spawnZ
        );
    }

    public void IncreaseObstacleDifficulty()
    {
        staticObstacleChance = Mathf.Min(
            staticObstacleChance + obstacleChanceIncrease,
            maxStaticObstacleChance
        );

        movingObstacleChance = Mathf.Min(
            movingObstacleChance + obstacleChanceIncrease,
            maxMovingObstacleChance
        );

        obstacleSpawnDistance = Mathf.Max(
            obstacleSpawnDistance - obstacleDistanceDecrease,
            minObstacleSpawnDistance
        );

        Debug.Log(
            "Obstacle Difficulty Increased! " +
            "Static Chance: " + staticObstacleChance +
            ", Moving Chance: " + movingObstacleChance +
            ", Distance: " + obstacleSpawnDistance
        );
    }
}
