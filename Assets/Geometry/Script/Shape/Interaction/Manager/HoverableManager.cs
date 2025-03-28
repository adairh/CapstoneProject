using System.Collections.Generic;
using UnityEngine;

public class HoverManager : MonoBehaviour
{
    public static HoverManager Instance { get; private set; }
    public bool AllMode = false;
    private HashSet<HoverableShape> hoveredObjects = new HashSet<HoverableShape>();
    private Shape lastShape = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterHoveredObject(HoverableShape obj)
    {
        hoveredObjects.Add(obj);
    }

    public void PinShape(Shape shape)
    {
        lastShape = shape;
    }

    public void UnpinShape()
    {
        lastShape = null;
    }

    public Shape GetPinnedShape()
    {
        return lastShape;
    }
    
    public void ResetAllHoveredObjects()
    {
        foreach (HoverableShape obj in hoveredObjects)
        {
            if (obj != null)
                obj.ResetHover(); // Reset object color & material
        }
        hoveredObjects.Clear();
    }
}