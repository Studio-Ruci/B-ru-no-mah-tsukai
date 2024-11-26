using UnityEngine;

public class KatamariController : MonoBehaviour
{
    public float torqueForce = 10f; // Torque force applied to the katamari
    public float rollSensitivity = 0.5f; // Sensitivity for rolling

    private Rigidbody rb; // Reference to the Rigidbody
    private bool isMobilePlatform; // To track if we are on mobile or Unity Remote

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Cache the Rigidbody component

        // Check if running on a mobile platform (real device or Unity Remote)
        isMobilePlatform = Application.isMobilePlatform;

        if (isMobilePlatform) // Check for mobile platform (real device)
        {
            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true; // Enable the gyroscope for mobile device
                Debug.Log("Gyroscope enabled.");
            }
            else
            {
                Debug.LogWarning("Gyroscope not supported on this device.");
            }
        }
        else
        {
            Debug.Log("Not a mobile platform, simulating gyroscope.");
        }
    }

    void Update()
    {
        // Get input for movement (keyboard or gyroscope, depending on device)
        float moveHorizontal = 0f;
        float moveVertical = 0f;

        // If on Unity Remote (simulating gyroscope), use keyboard or mouse input for testing
        if (!isMobilePlatform)
        {
            moveHorizontal = Input.GetAxis("Horizontal");
            moveVertical = Input.GetAxis("Vertical");
        }
        // If on a mobile device, use gyroscope for movement
        else if (isMobilePlatform && Input.gyro.enabled)
        {
            // Using gyroscope for movement (actual device)
            Vector3 gyroRotation = Input.gyro.rotationRateUnbiased;
            moveHorizontal = gyroRotation.y;  // Using gyroscope's yaw for horizontal movement
            moveVertical = gyroRotation.x;    // Using gyroscope's pitch for vertical movement
        }

        // Calculate torque based on input (keyboard or gyroscope)
        Vector3 torque = new Vector3(moveVertical, 0, -moveHorizontal) * torqueForce;

        // Apply torque to the Rigidbody
        rb.AddTorque(torque);

        // Adjust rolling sensitivity
        AdjustRolling();
    }

    private void AdjustRolling()
    {
        // Limit the speed to prevent excessive rolling
        if (rb.angularVelocity.magnitude > rollSensitivity)
        {
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, rollSensitivity);
        }
    }
}
