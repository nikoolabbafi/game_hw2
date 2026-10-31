using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLaneController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference laneLeftAction;
    [SerializeField] private InputActionReference laneRightAction;

    [Header("Lane Movement")]
    [SerializeField] private float laneWidth = 1.8f;
    [SerializeField] private float laneChangeSpeed = 5f;

    [Header("Forward Movement")]
    [SerializeField] private float forwardSpeed = 6f;

    // 0 = Left, 1 = Center, 2 = Right
    private int currentLane = 1;

    private Rigidbody rb;
    private float startX;

    public float ForwardSpeed => forwardSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startX = transform.position.x;
    }

    private void OnEnable()
    {
        if (laneLeftAction != null)
        {
            laneLeftAction.action.performed += MoveLeft;
            laneLeftAction.action.Enable();
        }

        if (laneRightAction != null)
        {
            laneRightAction.action.performed += MoveRight;
            laneRightAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (laneLeftAction != null)
        {
            laneLeftAction.action.performed -= MoveLeft;
            laneLeftAction.action.Disable();
        }

        if (laneRightAction != null)
        {
            laneRightAction.action.performed -= MoveRight;
            laneRightAction.action.Disable();
        }
    }

    private void MoveLeft(InputAction.CallbackContext context)
    {
        currentLane = Mathf.Max(0, currentLane - 1);
    }

    private void MoveRight(InputAction.CallbackContext context)
    {
        currentLane = Mathf.Min(2, currentLane + 1);
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        Vector3 nextPosition = rb.position;

      
        nextPosition.z += forwardSpeed * Time.fixedDeltaTime;

        // move between lanes
        float targetX = startX + (currentLane - 1) * laneWidth;

        nextPosition.x = Mathf.MoveTowards(
            rb.position.x,
            targetX,
            laneChangeSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);
    }

    public void SetForwardSpeed(float speed)
    {
        forwardSpeed = Mathf.Max(0f, speed);
    }
}

































