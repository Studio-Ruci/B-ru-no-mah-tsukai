using UnityEngine;
using UnityEngine.UI;

public class Size_Manager : MonoBehaviour
{
    // Reference to the ObjectCollector script to get the current size
    public ObjectCollector objectCollector;

    // Reference to the Text component in the Canvas
    public Text sizeText;

    // Public multiplier to adjust the size display, editable in the Inspector
    public float sizeMultiplier = 100f;

    // Public starting display value, editable in the Inspector
    public float startingDisplayValue = 0f;

    void Update()
    {
        // Check if references are properly assigned
        if (objectCollector == null)
        {
            Debug.LogError("ObjectCollector is not assigned in the Size_Manager script.");
            return;
        }

        if (sizeText == null)
        {
            Debug.LogError("SizeText is not assigned in the Size_Manager script.");
            return;
        }

        // Get the current size from the ObjectCollector script
        float currentSize = objectCollector.GetCurrentSize();

        // Manipulate the display value: apply multiplier and add the starting display value
        float displaySize = (currentSize * sizeMultiplier) + startingDisplayValue;

        // Format the number and append "cm"
        sizeText.text = $"{displaySize:F1} cm";  // Display with one decimal place
    }
}
