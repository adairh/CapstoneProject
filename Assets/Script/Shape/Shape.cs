using UnityEngine;
using System.Collections.Generic;

// Base abstract class for all shapes
public abstract class Shape
{
    public Vector3 Position { get; set; }
    public Color ShapeColor { get; set; }
    public string Name { get; set; }
    public bool IsSnappable { get; set; } = true; // Toggle Snap-to-Grid

    public Material DefaultMaterial { get; set; }
    public Material HighlightMaterial { get; set; }

    public EditableShape EditableShape;

    private GameObject go; // Private backing field
    public int id;

    public GameObject GO
    {
        get { return go; }
        set
        {
            go = value;
            RegisterEvents();
            go.name = Name + " " + ObjectCounter.Next();
            id = ObjectCounter.Current();
            //EditableShape = go.AddComponent<EditableShape>();
        }
    }

    // 🔥 List of settings for the shape
    protected List<ISetting> settings = new List<ISetting>();

    public Shape(Vector3 position, string name)
    {
        Position = position;
        ShapeColor = Color.red;
        Name = name;

        // ✅ Setup materials for hover effect
        DefaultMaterial = new Material(Shader.Find("Custom/SolidShader")) { color = ShapeColor };
        HighlightMaterial = new Material(Shader.Find("Custom/GlowingShader")) { color = ShapeColor };

        InitializeSettings(); // Initialize settings on creation
        
    }

    protected void RegisterEvents()
    {
        GO.AddComponent<ShapeClickHandler>().SetShape(this); // Link to this shape
        GO.AddComponent<HoverableShape>(); // Link to this shape
    }
    
    // 🔥 Abstract method: Each shape defines its own settings
    protected abstract void InitializeSettings();

    // 🔥 Allows child classes to append new settings
    public void AppendSettings(params ISetting[] newSettings)
    {
        settings.AddRange(newSettings);
    }

    // 🔥 Opens the settings panel
    public abstract void OpenConfigPanel();

    // 🔥 Applies the settings to the shape
    public virtual void ApplySettings()
    {
        foreach (ISetting setting in settings)
        {
            if (setting is ColorSetting colorSetting)
            {
                ShapeColor = colorSetting.Value;
                DefaultMaterial.color = ShapeColor; // Apply color change
            }
            else if (setting is PositionSetting positionSetting)
            {
                Position = positionSetting.Value;
                GO.transform.position = Position; // Move object
            }
        }
    }

    // 🔥 Updates shape when a setting is changed (for real-time updates)
    public virtual void OnSettingChanged(ISetting setting)
    {
        ApplySettings();
    }

    public abstract void Draw(); // General draw function

    // ✅ Return settings list
    public List<ISetting> GetSettings()
    {
        return settings;
    }
}


// Interface for 2D shapes
public interface IDrawable2D
{
    void Draw2D();
}

// Interface for 3D shapes
public interface IDrawable3D
{
    void Draw3D();
}

// Polygonal base class (Square, Triangle, Cube, etc.)
public abstract class PolygonalShape : Shape
{
    public PolygonalShape(Vector3 position, string name) : base(position, name) { }
}

// Circular base class (Circle, Sphere, etc.)
public abstract class CircularShape : Shape
{
    public CircularShape(Vector3 position, string name) : base(position, name) { }
}