using UnityEngine;

public class Cube : PolygonalShape, IDrawable3D
{
    public float Size { get; set; } 

    public Cube(Vector3 position, float size) : base(position, "Cube")
    {
        Size = size;
        Draw3D();
    }

    public void Draw3D()
    {
        if (GO == null)
        {
            GO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GO.transform.position = Position;
            GO.transform.localScale = Vector3.one * Size;
            
            // Set material color
            GO.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
            GO.GetComponent<Renderer>().material.color = ShapeColor;

            // Add interaction scripts
            /*GO.AddComponent<DraggableShape>();
            GO.AddComponent<ScalableShape>();
            GO.AddComponent<RotatableShape>();
            var hover = GO.AddComponent<HoverableShape>();
            hover.SetMaterials(DefaultMaterial, HighlightMaterial);*/
        }
    }

    public override void Draw() => Draw3D();
    protected override void InitializeSettings()
    {
        //throw new System.NotImplementedException();
    }
 
    public override void OpenConfigPanel()
    {
        //throw new System.NotImplementedException();
    }

    public void Sketch(Vector3 vector3, Camera mainCamera)
    {
        
    }
}