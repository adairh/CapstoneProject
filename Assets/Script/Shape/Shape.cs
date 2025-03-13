using UnityEngine;
using System.Collections.Generic;

// Base abstract class for all shapes

public static class ShapeStorage
{
    public static Dictionary<string, Shape> shapes = new Dictionary<string, Shape>();

    public static Shape GetShapeByID(string id)
    {
        return shapes[id];
    }
}
public abstract class Shape
{
    public Vector3 Position { get; set; }
    public Color ShapeColor { get; set; }
    public string Name { get; set; }
    public bool IsSnappable { get; set; } = true; // Toggle Snap-to-Grid

    public abstract GameObject[] Components();
    
    public Material DefaultMaterial { get; set; }
    public Material HighlightMaterial { get; set; }

    public EditableShape EditableShape;

    private GameObject go; // Private backing field
    public int id;
    public Shape shape;
    public Shape Parent { get; set; }

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

    public Shape(Vector3 position, string name, Shape parent)
    {
        Position = position;
        ShapeColor = Color.red;
        Name = name;

        // ✅ Setup materials for hover effect
        DefaultMaterial = new Material(Shader.Find("Custom/SolidShader")){ color = ShapeColor };
        HighlightMaterial = new Material(Shader.Find("Custom/GlowingShader")){ color = Color.cyan };

        Parent = parent;
        shape = this; 
        InitializeSettings();
        // Initialize settings on creation
    }

    protected void RegisterEvents()
    {
        GO.AddComponent<ShapeClickHandler>().SetShape(this); // Link to this shape
        
        HoverableShape hs = GO.AddComponent<HoverableShape>(); // Link to this shape
        hs.SetMaterials(this);
        
    }
    
    // 🔥 Abstract method: Each shape defines its own settings
    protected abstract void InitializeSettings();

    protected virtual void SetupGameObject()
    {
        ShapeStorage.shapes.Add(go.name, this);
    }

    // 🔥 Allows child classes to append new settings
    public void AppendSettings(params ISetting[] newSettings)
    {
        settings.AddRange(newSettings);
    }

    // 🔥 Opens the settings panel

    // 🔥 Applies the settings to the shape
    public virtual void ApplySettings()
    {
        
    }

    public void UpdateSettings(ISetting setting)
    {
        for (int i = 0; i < settings.Count; i++)
        {
            if (settings[i].GetType() == setting.GetType())
            {
                settings[i] = setting; // Replace with the new setting
                return; // Exit early after updating
            }
        }
    
        // If not found, add the new setting
        settings.Add(setting);
    }


    // 🔥 Updates shape when a setting is changed (for real-time updates)
    public virtual void OnSettingChanged(ISetting setting)
    {
        ApplySettings();
    }

    public void ModifySetting<T>(ISetting setting, T value)
    {
        setting.SetValue(value);
        UpdateSettings(setting);
        UpdateHitbox();
    }
    
    public abstract void UpdateHitbox(); // General draw function
    public abstract void Drawing(); // General draw function

    public void Draw()
    {
        SetIgnoreRaycast(true);
        Drawing();
    } // General draw function

    
    
    public virtual void CompleteDraw()
    {
        PerformDrawing.ResetShape(); 
        HoverableShape hs = GO.GetComponent<HoverableShape>();
        if (hs != null)
        {
            hs.SetComponents();
        }
        SetIgnoreRaycast(false);
    }
    public virtual void CompleteSettings()
    {
        
    }
    // General sketch function

    private const int IGNORE_RAYCAST_LAYER = 2; // Unity's built-in Ignore Raycast layer
    private int defaultLayer = 0; // Store original layer

    public void SetIgnoreRaycast(bool ignore)
    {
        if (GO == null) return;

        int targetLayer = ignore ? IGNORE_RAYCAST_LAYER : defaultLayer;

        // Change layer for the main object
        GO.layer = targetLayer;

        // Apply to all children recursively
        foreach (Transform child in GO.transform)
        {
            child.gameObject.layer = targetLayer;
        }
    }
    
    
    // ✅ Return settings list
    public List<ISetting> GetSettings()
    {
        return settings;
    }
    
    protected static Quaternion GetAlignedRotation(Camera mainCamera)
    {
        Vector3 forward = mainCamera.transform.forward;
        //forward.y = 0; // Remove vertical tilt to keep it on the XZ plane
        if (forward == Vector3.zero) forward = Vector3.forward; // Fallback

        return Quaternion.LookRotation(forward, Vector3.up);
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
    public PolygonalShape(Vector3 position, string name, Shape parent) : base(position, name, parent) { }
}

// Circular base class (Circle, Sphere, etc.)
public abstract class CircularShape : Shape
{
    public CircularShape(Vector3 position, string name, Shape parent) : base(position, name, parent) { }
    public float Radius { get; set; }
}