using UnityEngine;

public class ObjectScaler : MonoBehaviour
{
    private void Start()
    {
        // No changes to the visual scale are necessary, so we keep the initial scale as is
    }

    public void ScaleColliders(Transform katamari)
    {
        // Calculate the new scale factor based on the katamari's current scale
        float katamariScale = katamari.localScale.x; // Assuming uniform scaling

        // Adjust the scale of each collider attached to this object
        float scaleFactor = katamariScale / 2.0f; // Example scaling factor

        // Get all the collider components on the current object
        Collider[] colliders = GetComponents<Collider>();

        foreach (Collider collider in colliders)
        {
            // Scale BoxCollider
            if (collider is BoxCollider boxCollider)
            {
                boxCollider.size = boxCollider.size * scaleFactor;
            }
            // Scale SphereCollider
            else if (collider is SphereCollider sphereCollider)
            {
                sphereCollider.radius *= scaleFactor;
            }
            // Scale CapsuleCollider
            else if (collider is CapsuleCollider capsuleCollider)
            {
                capsuleCollider.height *= scaleFactor;
                capsuleCollider.radius *= scaleFactor;
            }
            // Scale MeshCollider (if convex and adjustable)
            else if (collider is MeshCollider meshCollider && meshCollider.convex)
            {
                // Use mesh scaling for convex colliders if necessary
                // In some cases, it might be better to adjust mesh bounds separately
            }
        }
    }
}
