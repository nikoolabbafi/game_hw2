using System.Globalization;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class SpeedDisplay : MonoBehaviour
{
    [SerializeField] private PlayerLaneController player;

    private TMPro.TextMeshProUGUI speedText;
    private int lastSpeedInTenths = -1;

    private void Awake()
    {
        speedText = GetComponent<TMPro.TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        lastSpeedInTenths = -1;
        RefreshSpeed();
    }

    private void LateUpdate()
    {
       
        RefreshSpeed();
    }

    private void RefreshSpeed()
    {
        if (player == null || speedText == null)
            return;

        int speedInTenths = Mathf.RoundToInt(player.ForwardSpeed * 10f);
        if (speedInTenths == lastSpeedInTenths)
            return;

        lastSpeedInTenths = speedInTenths;
        string value = (speedInTenths / 10f).ToString("0.0", CultureInfo.InvariantCulture);
        speedText.SetText("<size=60%>SPEED</size>  <b><mspace=0.65em>" + value + "</mspace></b>");
    }
}
