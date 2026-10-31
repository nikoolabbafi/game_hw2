using UnityEngine;

public class FuelCan : Interactable
{
    [SerializeField] private float fuelAmount = 25f;

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

        FuelSystem fuelSystem = player.GetComponent<FuelSystem>();

        if (fuelSystem != null)
        {
            float previousFuel = fuelSystem.CurrentFuel;
            fuelSystem.AddFuel(fuelAmount);
            if (fuelSystem.CurrentFuel > previousFuel)
                GameAudioManager.PlayFuelPickup();
        }

        Debug.Log("Fuel collected! +" + fuelAmount);

        GameVfxManager.PlayFuelPickup(gameObject);

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