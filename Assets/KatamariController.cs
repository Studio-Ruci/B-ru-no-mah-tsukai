using UnityEngine;

public class KatamariController : MonoBehaviour
{
    public float torqueForce = 10f; // Force applied to roll the Katamari
    public float turnSpeed = 2f; // Speed for turning the Katamari (customizable)
    public float rollSensitivity = 0.5f; // Limit for rolling speed
    public float pivotSpeed = 200f; // Camera pivot speed
    public Transform cameraTransform; // Reference to the camera

    private Rigidbody rb; // Rigidbody for the Katamari
    private Vector3 cameraOffset; // Initial camera offset
    private bool spacebarPressed = false; // Track if spacebar has been pressed

    // Flags for enabling the system without pressing spacebar
    private bool controlsEnabled = false;

    [Header("Movement Settings")]
    [SerializeField] private float forwardBackwardForce = 10f;  // Force applied for forward/backward movement (adjustable)
    [SerializeField, Range(0f, 360f)] private float movementRotationAngle = 90f;  // Adjustable rotation angle for forward/backward movement
    [SerializeField, Range(0f, 360f)] private float rollingRotationAngle = 90f;  // Adjustable rotation angle for rolling direction

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Cache Rigidbody

        // Store the initial camera offset
        if (cameraTransform != null)
        {
            cameraOffset = cameraTransform.position - transform.position;
        }

        // Enable gyroscope if supported
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }

        // Enable controls immediately on start
        controlsEnabled = true;
    }

    void Update()
    {
        // Only handle movement and rotation if controls are enabled
        if (controlsEnabled)
        {
            HandleMovement();
            HandleCameraPivot();
            HandleKatamariTurning(); // New turning logic
        }
    }

    private void HandleMovement()
    {
        // Handle forward/backward movement based on the camera's orientation
        float moveVertical = Input.GetAxis("Vertical"); // W = 1, S = -1

        // Add gyroscopic input if available
        if (SystemInfo.supportsGyroscope && Input.gyro.enabled)
        {
            moveVertical += Input.gyro.rotationRateUnbiased.x;
        }

        // Get the camera's forward and right directions (in world space)
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // Remove the Y component (since we don't want to move vertically)
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        // Normalize the vectors to prevent skewed movement
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Apply the adjustable rotation angle to the forward direction for movement
        cameraForward = Quaternion.Euler(0, movementRotationAngle, 0) * cameraForward; // Apply adjustable rotation angle for movement direction

        // Apply a force for forward/backward movement
        if (moveVertical != 0)
        {
            // Apply force in the forward direction (moving the Katamari)
            Vector3 forwardForce = cameraForward * moveVertical * forwardBackwardForce;
            rb.AddForce(forwardForce, ForceMode.Force);
        }

        // Apply torque for rolling, using the adjusted rolling direction
        Vector3 rollTorque = cameraForward * moveVertical * torqueForce; // Apply torque in the movement direction
        rollTorque = Quaternion.Euler(0, rollingRotationAngle, 0) * rollTorque; // Apply adjustable rolling rotation
        rb.AddTorque(rollTorque, ForceMode.Force);

        // Limit rolling speed (for smoothness)
        AdjustRolling();
    }

    private void HandleCameraPivot()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // A = -1, D = 1

        // Add gyroscopic input if available
        if (SystemInfo.supportsGyroscope && Input.gyro.enabled)
        {
            horizontalInput += Input.gyro.rotationRateUnbiased.y;
        }

        // Apply synchronized camera and ball turning
        if (horizontalInput != 0)
        {
            // Rotate the camera around the Katamari ball
            cameraTransform.RotateAround(transform.position, Vector3.up, horizontalInput * pivotSpeed * Time.deltaTime);
            cameraTransform.position = transform.position + cameraOffset;

            // Apply the same turning torque to the Katamari as the camera movement
            Vector3 turnTorqueVector = Vector3.up * horizontalInput * turnSpeed;
            rb.AddTorque(turnTorqueVector);
        }
        else
        {
            // Stop rotation immediately when there's no input
            rb.angularVelocity = Vector3.zero; // Set angular velocity to zero
        }
    }

    private void HandleKatamariTurning()
    {
        // No additional logic is needed for turning now; everything is handled in the pivot section.
    }

    private void AdjustRolling()
    {
        if (rb.angularVelocity.magnitude > rollSensitivity)
        {
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, rollSensitivity);
        }
    }
}
