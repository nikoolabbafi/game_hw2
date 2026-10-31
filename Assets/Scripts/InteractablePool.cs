using System.Collections.Generic;
using UnityEngine;

public class InteractablePool : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject fuelCanPrefab;
    [SerializeField] private GameObject staticObstaclePrefab;
    [SerializeField] private GameObject movingObstaclePrefab;

    [Header("Pool Settings")]
    [SerializeField] private int coinPoolSize = 10;
    [SerializeField] private int fuelCanPoolSize = 5;
    [SerializeField] private int staticObstaclePoolSize = 5;
    [SerializeField] private int movingObstaclePoolSize = 5;

    private List<GameObject> coinPool = new List<GameObject>();
    private List<GameObject> fuelCanPool = new List<GameObject>();
    private List<GameObject> staticObstaclePool = new List<GameObject>();
    private List<GameObject> movingObstaclePool = new List<GameObject>();

    private void Awake()
    {
        CreatePool(coinPrefab, coinPoolSize, coinPool);
        CreatePool(fuelCanPrefab, fuelCanPoolSize, fuelCanPool);
        CreatePool(
            staticObstaclePrefab,
            staticObstaclePoolSize,
            staticObstaclePool
        );
        CreatePool(
            movingObstaclePrefab,
            movingObstaclePoolSize,
            movingObstaclePool
        );
    }

    private void CreatePool(
        GameObject prefab,
        int amount,
        List<GameObject> pool)
    {
        if (prefab == null)
            return;

        for (int i = 0; i < amount; i++)
        {
            GameObject obj = Instantiate(prefab, transform);

            obj.SetActive(false);

            pool.Add(obj);
        }
    }

    public GameObject GetCoin()
    {
        return GetAvailableObject(coinPool);
    }

    public GameObject GetFuelCan()
    {
        return GetAvailableObject(fuelCanPool);
    }

    public GameObject GetStaticObstacle()
    {
        return GetAvailableObject(staticObstaclePool);
    }

    public GameObject GetMovingObstacle()
    {
        return GetAvailableObject(movingObstaclePool);
    }

    private GameObject GetAvailableObject(List<GameObject> pool)
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                Interactable interactable =
                    obj.GetComponent<Interactable>();

                if (interactable != null)
                    interactable.ResetInteractable();
                else
                    obj.SetActive(true);

                return obj;
            }
        }

        return null;
    }
}