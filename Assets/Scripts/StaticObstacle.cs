using UnityEngine;

public class StaticObstacle : Interactable
{
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float recycleDistance = 5f;

    private Transform player;
    private bool hasHitPlayer = false;

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

        if (transform.position.z < player.position.z - recycleDistance)
        {
            gameObject.SetActive(false);
        }
    }

    public override void Interact(GameObject playerObject)
    {
        if (hasHitPlayer)
            return;

        hasHitPlayer = true;

        Debug.Log(
            "Player hit static obstacle! Damage: " + damageAmount
        );

        
        // Player_Health
        PlayerHealth health = playerObject.GetComponent<PlayerHealth>();

        if (health != null)
        {
            Collider obstacleCollider = GetComponent<Collider>();
            Vector3 impactPosition = obstacleCollider != null
                ? obstacleCollider.ClosestPoint(playerObject.transform.position)
                : transform.position;
            health.TakeDamage(damageAmount, impactPosition);
        }
    }

    public override void ResetInteractable()
    {
        base.ResetInteractable();

        hasHitPlayer = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Interact(other.gameObject);
        }
    }
}