using UnityEngine;
using UnityEngine.UI;

public class FuelSystem : MonoBehaviour
{
    [Header("Fuel Settings")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float fuelDrainRate = 5f;

    [Header("Fuel UI")]
    [SerializeField] private Slider fuelBar;

    public float CurrentFuel { get; private set; }
    public float MaxFuel => maxFuel;

    private void Awake()
    {
        CurrentFuel = maxFuel;
    }

    private void Start()
    {
        UpdateFuelUI();
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
        {
            return;
        }

        DrainFuel();
    }

    private void DrainFuel()
    {
        CurrentFuel -= fuelDrainRate * Time.deltaTime;

        if (CurrentFuel < 0f)
            CurrentFuel = 0f;

        UpdateFuelUI();

        if (CurrentFuel <= 0f)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    public void AddFuel(float amount)
    {
        if (CurrentFuel <= 0f)
            return;

        CurrentFuel += amount;

        if (CurrentFuel > maxFuel)
            CurrentFuel = maxFuel;

        UpdateFuelUI();

        Debug.Log("Fuel: " + CurrentFuel);
    }

    private void UpdateFuelUI()
    {
        if (fuelBar == null)
            return;

        fuelBar.minValue = 0f;
        fuelBar.maxValue = maxFuel;
        fuelBar.value = CurrentFuel;
    }
}