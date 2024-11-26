using UnityEngine;

public class KatamariController : MonoBehaviour
{
    public float torqueForce = 10f; // Torque force applied to the katamari
    public float rollSensitivity = 0.5f; // Sensitivity for rolling

    private Rigidbody rb; // Reference to the Rigidbody

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Cache the Rigidbody component
    }

    void Update()
    {
        // Get input for movement
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Calculate torque based on input
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