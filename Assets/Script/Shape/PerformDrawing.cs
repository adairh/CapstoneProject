using UnityEngine;

public class PerformDrawing : MonoBehaviour
{
    public Camera mainCamera; // Assign in Inspector
    private IShapeButton.ShapeType currentShape = IShapeButton.ShapeType.None; // Track active shape

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

    private void DrawShape()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 v = Input.mousePosition;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            switch (currentShape)
            {
                case IShapeButton.ShapeType.Circle:
                    Circle.Sketch(hit.point, v, mainCamera);
                    break;
                case IShapeButton.ShapeType.Rectangle:
                    Rectangle.Sketch(hit.point, v, mainCamera);
                    break;
            }
        }
    }
}
