using UnityEngine;

public class Coin : Interactable
{
    [SerializeField] private int scoreValue = 10;

    private Transform player;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void Update()
    {
        if (player == null)
            return;

        if (transform.position.z < player.position.z - 5f)
        {
            gameObject.SetActive(false);
        }
    }

    public override void Interact(GameObject player)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        GameAudioManager.PlayCoinPickup();

        Debug.Log("Coin collected! +" + scoreValue + " score");

        GameVfxManager.PlayCoinPickup(gameObject);

        gameObject.SetActive(false);
    }

    public override void ResetInteractable()
    {
        base.ResetInteractable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Interact(other.gameObject);
        }
    }
}