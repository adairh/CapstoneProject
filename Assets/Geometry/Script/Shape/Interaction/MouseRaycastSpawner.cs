using UnityEngine;

public class MouseRaycastSpawner : MonoBehaviour
{
    public Camera mainCamera; // Assign the camera in the Inspector

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) // Check if the ray hits something
            {
                SpawnSphere(hit.point);
            }
        }
    }

    void SpawnSphere(Vector3 position)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * 0.5f; // Adjust size if needed

    }
}