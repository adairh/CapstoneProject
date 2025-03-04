using UnityEngine;

public class ClickToWorldPosition : MonoBehaviour
{
    void Update()
    {
        // Check if the user clicked the mouse
        if (Input.GetMouseButtonDown(0)) // 0 = Left mouse button
        {
            // Get the mouse position in screen space
            Vector3 screenPosition = Input.mousePosition;

            // Convert the screen position to world position
            // Assuming the click is on the ground or a specific Z-value in world space
            screenPosition.z = 10f;  // Set the Z value to a specific value (distance from the camera)
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            // Log or use the world position
            Debug.Log("World Position: " + worldPosition);
        }
    }
}