
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLaneController player;
    [SerializeField] private InteractableSpawner obstacleSpawner;

    [Header("Difficulty Settings")]
    [SerializeField] private float increaseInterval = 15f;
    [SerializeField] private float distancePerDifficultyStep = 120f;
    [SerializeField] private float speedIncreaseAmount = 0.5f;
    [SerializeField] private float maxPlayerSpeed = 15f;

    private float difficultyTimer;
    private float lastDifficultyZ;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError(
                "DifficultyManager: Player is not assigned."
            );

            enabled = false;
            return;
        }

        lastDifficultyZ = player.transform.position.z;
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
        {
            return;
        }

        difficultyTimer += Time.deltaTime;

        float distanceSinceLastStep =
            player.transform.position.z - lastDifficultyZ;

        bool timeReached =
            difficultyTimer >= increaseInterval;

        bool distanceReached =
            distanceSinceLastStep >= distancePerDifficultyStep;

        if (timeReached || distanceReached)
        {
            difficultyTimer = 0f;
            lastDifficultyZ = player.transform.position.z;

            IncreaseDifficulty();
        }
    }

    private void IncreaseDifficulty()
    {
        float newSpeed = Mathf.Min(
            player.ForwardSpeed + speedIncreaseAmount,
            maxPlayerSpeed
        );

        player.SetForwardSpeed(newSpeed);

        if (obstacleSpawner != null)
        {
            obstacleSpawner.IncreaseObstacleDifficulty();
        }

        Debug.Log(
            "Difficulty increased! " +
            "Player Speed: " + newSpeed +
            ", Distance: " + player.transform.position.z
        );
    }
}
