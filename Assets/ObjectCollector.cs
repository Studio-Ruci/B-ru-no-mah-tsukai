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

    void Start()
    {
        // Initialize the Sphere Collider size to match the katamari's size
        collectionCollider.radius = katamari.localScale.x / 2;
    }

    void Update()
    {
        // Continuously reset the size of collected objects to their original size every frame
        ResetCollectedObjectsSize();

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
            // Handle the detachment, growth, and reattachment
            collectedObjects.Add(other);
            StartCoroutine(HandleObjectCollection(other));
        }
    }

    IEnumerator HandleObjectCollection(Collider objectCollider)
    {
        // Store the original size of the object when first collected
        if (!originalObjectSizes.ContainsKey(objectCollider))
        {
            originalObjectSizes[objectCollider] = objectCollider.transform.localScale;
        }

        // Detach the object to avoid it growing with the katamari
        DetachObject(objectCollider);

        // After detachment, allow the katamari to grow
        canGrow = true;
        yield return new WaitForSeconds(0.1f);

        // Only after detachment, grow the Katamari
        GrowKatamari();

        // Reattach the object after the katamari has grown
        yield return new WaitForSeconds(0.1f);
        ReattachObject(objectCollider);
    }

    public void DetachObject(Collider objectCollider)
    {
        // Detach the object from katamari to prevent size scaling
        Transform objectTransform = objectCollider.transform;
        objectTransform.parent = null;

        // Disable the collider temporarily to avoid further interactions
        Collider objectCol = objectTransform.GetComponent<Collider>();
        if (objectCol != null)
        {
            objectCol.enabled = false;
        }

        // Ensure the size of the object stays the same while detached
        objectTransform.localScale = originalObjectSizes[objectCollider];
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

    public void ResetCollectedObjectsSize()
    {
        // Continuously reset the size of all collected objects to their original size
        foreach (Collider obj in collectedObjects)
        {
            if (obj != null)
            {
                // Get the original size of the object
                Vector3 originalSize = originalObjectSizes[obj];

                // Reset the object's size to its original size every frame
                obj.transform.localScale = originalSize;
            }
        }
    }

    public void ReattachObject(Collider objectCollider)
    {
        // Re-parent the object to the katamari
        objectCollider.transform.parent = katamari;

        // Optionally set the position to make it attach nicely
        Vector3 position = katamari.position + (katamari.localScale.y / 2) * Vector3.up + new Vector3(0, offset, 0);
        objectCollider.transform.position = position;

        // Permanently disable the collider once attached
        Collider objectCol = objectCollider.GetComponent<Collider>();
        if (objectCol != null)
        {
            objectCol.enabled = false;
        }

        // Make sure the object size is reset again once reattached
        objectCollider.transform.localScale = originalObjectSizes[objectCollider];

        // Add the object to the queue of attached objects
        attachedObjects.Enqueue(objectCollider);

        // If there are more than maxAttachedObjects, remove the first one attached
        if (attachedObjects.Count > maxAttachedObjects)
        {
            Collider firstObject = attachedObjects.Dequeue();
            RemoveObjectFromKatamari(firstObject);
        }
    }

    public void RemoveObjectFromKatamari(Collider objectCollider)
    {
        // Remove the object from the katamari and reset its position and state
        Transform objectTransform = objectCollider.transform;
        objectTransform.parent = null;

        // Re-enable the collider so it can interact with the world again
        Collider objectCol = objectTransform.GetComponent<Collider>();
        if (objectCol != null)
        {
            objectCol.enabled = true;
        }

        // Reset the size to its original value
        objectTransform.localScale = originalObjectSizes[objectCollider];

        // Optionally destroy the object (or pool it for reuse)
        Destroy(objectCollider.gameObject);
    }

    // Public getter for currentSize
    public float GetCurrentSize()
    {
        return currentSize;
    }
}
 