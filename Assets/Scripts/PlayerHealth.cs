using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 3;

    [Header("Lives UI")]
    [SerializeField] private GameObject[] lifeIcons;

    public int CurrentLives { get; private set; }

    private void Awake()
    {
        CurrentLives = maxLives;

        UpdateLivesUI();
    }

    public void TakeDamage(int damageAmount)
    {
        TakeDamage(damageAmount, transform.position + transform.forward * 0.32f + Vector3.up * 0.1f);
    }

    public void TakeDamage(int damageAmount, Vector3 impactPosition)
    {
        if (CurrentLives <= 0)
            return;

        CurrentLives -= damageAmount;

        if (damageAmount > 0)
        {
            GameAudioManager.PlayCollision();
            GameVfxManager.PlayCollision(impactPosition);
        }

        if (CurrentLives < 0)
            CurrentLives = 0;

        Debug.Log("Player Lives: " + CurrentLives);

        UpdateLivesUI();

        PlayerDamageFeedback feedback =
            GetComponent<PlayerDamageFeedback>();

        if (feedback != null)
        {
            feedback.PlayDamageFeedback();
        }

        if (CurrentLives <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    private void UpdateLivesUI()
    {
        if (lifeIcons == null)
            return;

        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
            {
                lifeIcons[i].SetActive(i < CurrentLives);
            }
        }
    }
}