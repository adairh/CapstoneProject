using System;
using UnityEngine;

public static class ShapeButtonManager
{
    public static event Action<IShapeButton.ShapeType> OnShapeChanged;
    
    private static IShapeButton.ShapeType activeType = IShapeButton.ShapeType.None;
    public static IShapeButton.ShapeType ActiveType
    {
        get => activeType;
        private set
        {
            if (activeType != value)
            {
                activeType = value;
                Debug.Log($"[ShapeButtonManager] Active Shape Set To: {activeType}");
                OnShapeChanged?.Invoke(activeType); // Notify listeners
            }
        }
    }

    public static void SetActiveShape(IShapeButton.ShapeType newType)
    {
        ActiveType = newType;
    }
}