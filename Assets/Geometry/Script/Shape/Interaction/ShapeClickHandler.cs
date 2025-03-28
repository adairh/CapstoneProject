using UnityEngine;

public class ShapeClickHandler : MonoBehaviour
{
    private Shape shape;
    private SpawnPanel panelSpawner;

    public void SetShape(Shape shape)
    {
        this.shape = shape;
        while (this.shape.Parent != null)
        {
            this.shape = this.shape.Parent;
        }
    }

    private void Start()
    {
        panelSpawner = new SpawnPanel();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) // Right-click
        {
            panelSpawner.SpawnPanelAtTop(shape);
        }
    }
}
