using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCollector : MonoBehaviour
{
    public Transform katamari;                      // Reference to the katamari object
    public SphereCollider collectionCollider;       // The Sphere Collider used for object collection
    public float offset = 0.1f;                     // Offset to position the object slightly above the surface
    public float growthRate = 0.05f;                // Rate at which the katamari grows
    private float currentSize = 1f;                 // Current size of the katamari
    private HashSet<Collider> collectedObjects = new HashSet<Collider>(); // Track collected objects
    private Queue<Collider> attachedObjects = new Queue<Collider>();    // Track the order of attached objects
    private Dictionary<Collider, Vector3> originalObjectSizes = new Dictionary<Collider, Vector3>(); // Store original sizes of objects
    private bool canGrow = true;                    // Flag to determine if katamari can grow

    [Header("Settings")]
    [Tooltip("Max number of objects the Katamari can collect at once.")]
    [Range(1, 100)]
    public int maxAttachedObjects = 60;            // Max number of objects to be attached (editable in the editor)

    [Header("Collection Size Threshold")]
    [Range(1, 100)]
    public float collectibleSizeThreshold = 40f;  // Threshold for object size (in percentage)

    void Start()
    {
        // Initialize the Sphere Collider size to match the katamari's size
        collectionCollider.radius = katamari.localScale.x / 2;
    }

    void Update()
    {
        // Optional: Space bar for manual testing of katamari growth
        if (Input.GetKey(KeyCode.Space) && canGrow)
        {
            GrowKatamari();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Ensure only objects with the tag "Collectible" are processed
        if (other.CompareTag("Collectible") && !collectedObjects.Contains(other))
        {
            // Get the size of the collectible object
            float objectSize = other.transform.localScale.x;  // Assuming uniform scaling for simplicity

            // Calculate the threshold size (e.g., 40% of katamari's current size)
            float katamariSizeThreshold = currentSize * (collectibleSizeThreshold / 100f);

            // If the object's size is less than or equal to the threshold, proceed with collection
            if (objectSize <= katamariSizeThreshold)
            {
                // Handle the detachment, growth, and reattachment
                collectedObjects.Add(other);
                StartCoroutine(HandleObjectCollection(other));
            }
        }
    }

    IEnumerator HandleObjectCollection(Collider objectCollider)
    {
        // Store the original size of the object when first collected
        if (!originalObjectSizes.ContainsKey(objectCollider))
        {
            originalObjectSizes[objectCollider] = objectCollider.transform.localScale;
        }

        // Detach the object before growing the katamari
        DetachObject(objectCollider);

        // Allow the katamari to grow
        canGrow = true;
        yield return new WaitForSeconds(0.1f);

        // After detachment, grow the Katamari
        GrowKatamari();

        // Reattach the object after the katamari has grown
        yield return new WaitForSeconds(0.1f);
        ReattachObject(objectCollider);
    }

    public void DetachObject(Collider objectCollider)
    {
        // Check if the objectCollider is null or destroyed
        if (objectCollider == null)
        {
            Debug.LogWarning("Attempted to detach a null or destroyed object.");
            return;
        }

        Transform objectTransform = objectCollider.transform;

        // Detach the object by setting its parent to null
        objectTransform.parent = null;

        // Reset the object's size to its original size
        if (originalObjectSizes.ContainsKey(objectCollider))
        {
            objectTransform.localScale = originalObjectSizes[objectCollider];
        }

        // Optionally disable the collider temporarily to avoid any other interactions
        Collider objectCol = objectTransform.GetComponent<Collider>();
        if (objectCol != null)
        {
            objectCol.enabled = false;
        }
    }

    public void GrowKatamari()
    {
        // Ensure the katamari grows only if it's allowed
        if (canGrow)
        {
            // Increase the katamari's size based on the growthRate
            currentSize += growthRate;
            katamari.localScale = new Vector3(currentSize, currentSize, currentSize);

            // Update the collider size to match the new katamari size
            collectionCollider.radius = currentSize / 2;

            // Disable growth flag to prevent further immediate growth
            canGrow = false;
        }
    }

    public void ReattachObject(Collider objectCollider)
    {
        // Check if the objectCollider is null or destroyed
        if (objectCollider == null)
        {
            Debug.LogWarning("Attempted to reattach a null or destroyed object.");
            return;
        }

        // Re-parent the object to the katamari
        objectCollider.transform.parent = katamari;

        // Position the object to fill gaps on the katamari surface
        Vector3 position = CalculateOptimalPosition(objectCollider);
        objectCollider.transform.position = position;

        // Permanently disable the collider once attached
        Collider objectCol = objectCollider.GetComponent<Collider>();
        if (objectCol != null)
        {
            objectCol.enabled = false;
        }

        // Make sure the object size is reset again once reattached
        if (originalObjectSizes.ContainsKey(objectCollider))
        {
            objectCollider.transform.localScale = originalObjectSizes[objectCollider];
        }

        // Add the object to the queue of attached objects
        attachedObjects.Enqueue(objectCollider);

        // If there are more than maxAttachedObjects, remove the first one attached
        if (attachedObjects.Count > maxAttachedObjects)
        {
            Collider firstObject = attachedObjects.Dequeue();
            RemoveObjectFromKatamari(firstObject);
        }
    }

    Vector3 CalculateOptimalPosition(Collider objectCollider)
    {
        // Calculate position on the katamari's surface based on its size and offset
        Vector3 direction = (objectCollider.transform.position - katamari.position).normalized;
        return katamari.position + (katamari.localScale.y / 2 + offset) * direction;
    }

    public void RemoveObjectFromKatamari(Collider objectCollider)
    {
        // Check if the objectCollider is null or destroyed
        if (objectCollider == null)
        {
            Debug.LogWarning("Attempted to remove a null or destroyed object from the katamari.");
            return;
        }

        Transform objectTransform = objectCollider.transform;

        // Re-enable the collider so it can interact with the world again
        Collider objectCol = objectTransform.GetComponent<Collider>();
        if (objectCol != null)
        {
            objectCol.enabled = true;
        }

        // Reset the size to its original value
        if (originalObjectSizes.ContainsKey(objectCollider))
        {
            objectTransform.localScale = originalObjectSizes[objectCollider];
        }

        // Optionally destroy the object (or pool it for reuse)
        Destroy(objectCollider.gameObject);

        // Reorganize attached objects to fill the gap left by the removed object
        ReorganizeAttachedObjects();
    }

    void ReorganizeAttachedObjects()
    {
        // Check the positions of all attached objects and fill any gaps created
        foreach (Collider obj in attachedObjects)
        {
            if (obj != null)
            {
                Vector3 newPos = CalculateOptimalPosition(obj);
                obj.transform.position = newPos;
            }
        }
    }

    // Public getter for currentSize
    public float GetCurrentSize()
    {
        return currentSize;
    }
}
