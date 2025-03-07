using UnityEngine;

public class Sphere : CircularShape, IDrawable3D
{
    public float Radius { get; set; } // Radius should only be set via settings

    public Sphere(Vector3 position, float radius) : base(position, "Sphere")
    {
        Radius = radius;
        Draw3D();
        InitializeSettings(); // ✅ Ensure settings are initialized
    }

    public void Draw3D()
    {
        if (GO == null)
        {
            GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GO.transform.position = Position;
            GO.transform.localScale = Vector3.one * (Radius * 2); 

            // Set material color
            GO.GetComponent<Renderer>().material = DefaultMaterial;

            // Add interaction scripts
            /*GO.AddComponent<DraggableShape>();
            GO.AddComponent<ScalableShape>();
            GO.AddComponent<RotatableShape>();
            var hover = GO.AddComponent<HoverableShape>();
            hover.SetMaterials(DefaultMaterial, HighlightMaterial);*/

        }
    }

    public override void Draw() => Draw3D();

    // 🔥 Define shape-specific settings (overrides abstract method in Shape)
    protected override void InitializeSettings()
    {
        AppendSettings(
            new ColorSetting(ShapeColor),
            new RadiusSetting(10f),
            new PositionSetting(Position)
        );
    }

    // 🔥 Update shape when a setting changes (e.g., size, position, color)
    public override void OnSettingChanged(ISetting setting)
    {
        if (setting is ColorSetting colorSetting)
        {
            ShapeColor = colorSetting.Value;
            DefaultMaterial.color = ShapeColor;
        }
        else if (setting is RadiusSetting radiusSetting)
        {
            Radius = radiusSetting.Value;
            GO.transform.localScale = Vector3.one * (Radius * 2); // ✅ Update size instantly
        }
        else if (setting is PositionSetting positionSetting)
        {
            Position = positionSetting.Value;
            GO.transform.position = Position;
        }
    }

    // 🔥 Open the right-click settings panel
    public override void OpenConfigPanel()
    {
        Debug.Log("Opening Sphere Config Panel...");
        //UIManager.Instance.ShowConfigPanel(this);
    }

    public void Sketch(Vector3 vector3, Camera mainCamera)
    {
        
    }
}
