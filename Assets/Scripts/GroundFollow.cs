using UnityEngine;

public class GroundFollow : MonoBehaviour
{
    public Transform player;

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 newPosition = transform.position;
        newPosition.z = player.position.z;

        transform.position = newPosition;
    }
}