using UnityEngine;
using System.Collections.Generic;

public class ShapeManager : MonoBehaviour
{
    private List<Shape> shapes = new List<Shape>();

    void Start()
    {
        // Example Usage
        // AddShape(new Rectangle(new Vector3(2, 2, 2), 2f, 3f));
        // AddShape(new Triangle(new Vector3(2, 2,2),new Vector3(3, 3, 3),new Vector3(4, 2,2)));
        // AddShape(new Circle(new Vector3(2, 2, 2), 3f));
        // AddShape(new Cube(new Vector3(0, 0, 0), 1f));
        // AddShape(new Sphere(new Vector3(4, 0, 5), 2f));
        // AddShape(new Pyramid(new Vector3(4, 0, 5)));

        DrawAllShapes();
    }

    void AddShape(Shape shape)
    {
        if (shape.IsSnappable)
        {
            shape.Position = SnapToGrid(shape.Position);
        }

        shapes.Add(shape);
    }

    void DrawAllShapes()
    {
        foreach (var shape in shapes)
        {
            //shape.Draw();
        }
    }

    Vector3 SnapToGrid(Vector3 position)
    {
        float gridSize = 1f;
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize,
            Mathf.Round(position.z / gridSize) * gridSize
        );
    }
}