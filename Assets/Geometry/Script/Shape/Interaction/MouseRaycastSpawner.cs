using UnityEngine;

public class MouseRaycastSpawner : MonoBehaviour
{
    public Camera mainCamera; // Assign the camera in the Inspector

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) // Check if the ray hits something
                SpawnSphere(hit.point);
        }
    }

    private void SpawnSphere(Vector3 position)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * 0.5f; // Adjust size if needed
    }
}