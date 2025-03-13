using UnityEngine;
using Plane = System.Numerics.Plane;

public class PerformDrawing : MonoBehaviour
{
    public Camera mainCamera; // Assign in Inspector
    private static IShapeButton.ShapeType currentShape = IShapeButton.ShapeType.None; // Track active shape

    void Start()
    {
        ShapeButtonManager.OnShapeChanged += HandleShapeChange;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        ShapeButtonManager.OnShapeChanged -= HandleShapeChange;
    }

    void HandleShapeChange(IShapeButton.ShapeType newShape)
    {
        Debug.Log($"[PerformDrawing] Shape changed to: {newShape}");
        currentShape = newShape; // Update the active shape
    }

    void Update()
    {
        if (mainCamera == null) return;
        if (currentShape == IShapeButton.ShapeType.None) return; // Do nothing if no shape selected
        DrawShape();
    }

    public static void ResetShape()
    {
        currentShape = IShapeButton.ShapeType.None;
        ShapeButtonManager.SetActiveShape(IShapeButton.ShapeType.None);
    }
    
    private void DrawShape()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 v = Input.mousePosition;
        Vector3 hitPoint;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hitPoint = hit.point; // Snap to the object it hit
        }
        else
        {
            UnityEngine.Plane groundPlane = new UnityEngine.Plane(Vector3.up, Vector3.zero); // Assume Y=0 ground plane
            if (groundPlane.Raycast(ray, out float enter))
            {
                hitPoint = ray.GetPoint(enter); // Use the ground plane intersection
            }
            else
            {
                return; // No valid placement point, exit early
            }
        }

        switch (currentShape)
        {
            case IShapeButton.ShapeType.Circle:
                Circle.Sketch(hitPoint, v, mainCamera);
                break;
            case IShapeButton.ShapeType.Rectangle:
                Rectangle.Sketch(hitPoint, v, mainCamera);
                break;
            case IShapeButton.ShapeType.Triangle:
                Triangle.Sketch(hitPoint, v, mainCamera);
                break;
        }
    }


    
}
