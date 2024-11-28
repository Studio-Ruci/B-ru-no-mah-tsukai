using UnityEngine;

public class KatamariController : MonoBehaviour
{
    public float torqueForce = 10f;              // Force applied to roll the Katamari
    public float turnSpeed = 2f;                 // Speed for turning the Katamari
    public float rollSensitivity = 0.5f;         // Limit for rolling speed
    public float pivotSpeed = 200f;              // Camera pivot speed
    public Transform cameraTransform;            // Reference to the camera

    private Rigidbody rb;                        // Rigidbody for the Katamari
    private Vector3 cameraOffset;                // Initial camera offset

    [Header("Movement Settings")]
    [SerializeField] private float forwardBackwardForce = 10f;
    [SerializeField, Range(0f, 360f)] private float movementRotationAngle = 90f;
    [SerializeField, Range(0f, 360f)] private float rollingRotationAngle = 90f;

    [Header("Toggles")]
    public bool enableMovementForce = true;      // Toggle for movement force
    public bool enableRolling = true;            // Toggle for rolling torque

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Store the initial offset between camera and Katamari
        if (cameraTransform != null)
        {
            cameraOffset = cameraTransform.position - transform.position;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleCameraPivot();
    }

    private void HandleMovement()
    {
        float moveVertical = Input.GetAxis("Vertical");

        if (enableMovementForce && moveVertical != 0)
        {
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();
            cameraForward = Quaternion.Euler(0, movementRotationAngle, 0) * cameraForward;

            // Apply forward/backward force
            Vector3 forwardForce = cameraForward * moveVertical * forwardBackwardForce;
            rb.AddForce(forwardForce, ForceMode.Force);
        }

        if (enableRolling && moveVertical != 0)
        {
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();
            cameraForward = Quaternion.Euler(0, rollingRotationAngle, 0) * cameraForward;

            // Apply rolling torque
            Vector3 rollTorque = cameraForward * moveVertical * torqueForce;
            rb.AddTorque(rollTorque, ForceMode.Force);
        }

        AdjustRolling();
    }

    private void HandleCameraPivot()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput != 0)
        {
            cameraTransform.RotateAround(transform.position, Vector3.up, horizontalInput * pivotSpeed * Time.deltaTime);
            cameraOffset = cameraTransform.position - transform.position;
        }

        Vector3 targetPosition = transform.position + cameraOffset;
        targetPosition.y = transform.position.y + cameraOffset.y;
        cameraTransform.position = targetPosition;

        cameraTransform.LookAt(transform);
    }

    private void AdjustRolling()
    {
        if (rb.angularVelocity.magnitude > rollSensitivity)
        {
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, rollSensitivity);
        }
    }
}