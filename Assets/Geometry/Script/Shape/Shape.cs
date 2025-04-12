using System;
using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Manipulator
{
// Base abstract class for all shapes

    public static class ShapeStorage
    {
        private static Dictionary<string, Shape> shapes = new Dictionary<string, Shape>();

        public static Shape GetShapeByID(string id)
        {
            return shapes[id];
        }

        public static void RemoveShape(string id)
        {
            shapes.Remove(id);
        }

        public static void AddShape(string id, Shape shape)
        {
            shapes.Add(id, shape);
        }
        
        public static IEnumerable<Shape> GetAllShapes()
        {
            return shapes.Values;
        }

        public static Point FindNearestPoint(Vector3 position, float maxSnapDistance = 0.1f)
        {
            Point closest = null; 
            float closestSqrDistance = maxSnapDistance * maxSnapDistance;

            foreach (var shape in GetAllShapes())
            { 
                if (shape is Point point && point.IsSnappable && point.GO.layer != 2)
                {
                    float sqrDist = (point.Position - position).sqrMagnitude;

                    if (sqrDist < closestSqrDistance)
                    {
                        closest = point;
                        closestSqrDistance = sqrDist;
                    }
                }
            }

            return closest;
        }




    }

    public abstract class Shape
    {

        private Dictionary<Point, RatioCalculator> DependentPoints = new Dictionary<Point, RatioCalculator>();

        private List<Point> PivotPoints = new List<Point>();

        public Vector3 Position { get; internal set; }
        public Color ShapeColor { get; set; }
        public string Name { get; set; }
        public bool IsSnappable { get; set; } = true; // Toggle Snap-to-Grid

        public abstract GameObject[] Components();

        public void AdjustToPosition(Vector3 vector3, bool transform = true)
        {
            Position = vector3;
            if (transform)
            {
                GO.transform.position = vector3;
            }
        }


        public void MoveToPosition(Vector3 vector)
        {
            AdjustToPosition(vector);
            CompleteSettings();
            Draw();
            UpdateHitbox(); 
            CompleteDraw();
        }

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
                go.name = Name; 
                //EditableShape = go.AddComponent<EditableShape>();
            }
        }

        // 🔥 List of settings for the shape
        protected List<ISetting> settings = new List<ISetting>();

        public Shape(Vector3 position, string name, Shape parent)
        {
            Position = position;
            ShapeColor = Color.red;
            Name = name + " " + ObjectCounter.Next();
            id = ObjectCounter.Current();

            // ✅ Setup materials for hover effect
            DefaultMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = ShapeColor };
            HighlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = Color.cyan };

            Parent = parent;
            shape = this;
            
            ShapeStorage.AddShape(Name, this);
            InitializeSettings();
            // Initialize settings on creation
            
            
        }

        protected void RegisterEvents()
        {
            GO.AddComponent<ShapeClickHandler>().SetShape(this); // Link to this shape

            GO.tag = (Parent == null) ? "Shape" : "Child";

            GO.AddComponent<DraggableShape>().SetShape(this);

            GO.AddComponent<HoverableShape>().SetMaterials(this);
             

        }
        
        
        public virtual void OnPointMoved(Point movedPoint)
        {
            // Default behavior: Do nothing
        }
        
        
        // 🔥 Abstract method: Each shape defines its own settings
        protected abstract void InitializeSettings();

        protected virtual void SetupGameObject()
        {
            
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

        public void UpdateParent(Shape shape)
        {
            Parent = shape;

            GO.transform.SetParent(Parent.GO.transform, true); // Keep world position
            //GO.transform.position = Position; // Ensure world position is correct

            // ✅ Detach from parent scaling while keeping position
            //GO.transform.SetParent(null, true);

            GO.tag = (Parent == null) ? "Shape" : "Child";
            Drawing();
            //UpdateHitbox();
        }

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

            //Debug.LogWarning($"{Name} Set to {ignore} raycast");
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


        public void Destroy()
        { 
            
            if (ShapeStorage.GetShapeByID(GO.name) != null)
            {
                ShapeStorage.RemoveShape(GO.name);
            }
            Object.Destroy(GO); 
        }

        public List<Point> GetPivots()
        {
            return PivotPoints;
        }
        
        public void AddPivot(Point point)
        {
            if (!IsPivot(point))
            {
                PivotPoints.Add(point);
            }
        }

        public void RemovePivot(Point point)
        {
            if (PivotPoints.Contains(point))
            {
                PivotPoints.Remove(point);
            }
        }

        public bool IsPivot(Point point)
        {
            return PivotPoints.Contains(point);
        }
        
        

        public Dictionary<Point, RatioCalculator> GetDependencies()
        {
            return DependentPoints;
        }

        public void AddDepend(Point point)
        {
            if (!IsDepend(point) && PivotPoints.Count > 0)
                DependentPoints.Add(point, new RatioCalculator(point, PivotPoints));
        }

        public void RemoveDepend(Point point)
        {
            if (IsDepend(point))
                DependentPoints.Remove(point);
        }

        public bool IsDepend(Point point)
        {
            return DependentPoints.ContainsKey(point);
        }

        public RatioCalculator GetDependData(Point point)
        {
            if (IsDepend(point))
                return DependentPoints[point];
            return null;
        }

        
        public class RatioCalculator
        {
            private Point _point;
            private List<Point> _pivots;
            private Dictionary<Point, Tuple<Vector3, float>> data;

            public RatioCalculator(Point point, List<Point> pivots)
            {
                _point = point;
                _pivots = new List<Point>(pivots); // Copy to avoid reference issues
                data = new Dictionary<Point, Tuple<Vector3, float>>();
                Calculate();
            }

            /// <summary>
            /// Recalculate all ratios between the base point and its pivots.
            /// </summary>
            private void Calculate()
            {
                data.Clear();
                foreach (Point p in _pivots)
                {
                    Vector3 nav = (_point.Position - p.Position).normalized;
                    float dis = Vector3.Distance(_point.Position, p.Position);
                    data[p] = Tuple.Create(nav, dis);
                }
            }

            /// <summary>
            /// Get the ratio data: direction vector and distance for each pivot.
            /// </summary>
            public Dictionary<Point, Tuple<Vector3, float>> GetData() => data;

            /// <summary>
            /// Returns a copy of the pivot list.
            /// </summary>
            public List<Point> GetPivots() => new List<Point>(_pivots);

            /// <summary>
            /// Adds a pivot and updates the ratio.
            /// </summary>
            public void AddPivot(Point p)
            {
                if (!_pivots.Contains(p))
                {
                    _pivots.Add(p);
                    UpdatePivot(p);
                }
            }

            /// <summary>
            /// Removes a pivot and clears its ratio.
            /// </summary>
            public void RemovePivot(Point p)
            {
                if (_pivots.Remove(p))
                {
                    data.Remove(p);
                }
            }

            /// <summary>
            /// Updates the ratio for a specific pivot only.
            /// </summary>
            public void UpdatePivot(Point p)
            {
                if (_pivots.Contains(p))
                {
                    Vector3 nav = (_point.Position - p.Position).normalized;
                    float dis = Vector3.Distance(_point.Position, p.Position);
                    data[p] = Tuple.Create(nav, dis);
                }
            }

            /// <summary>
            /// Refresh all data, useful if the main point has moved.
            /// </summary>
            public void Refresh()
            {
                Calculate();
            }

            /// <summary>
            /// Returns true if the pivot exists.
            /// </summary>
            public bool HasPivot(Point p) => _pivots.Contains(p);

            /// <summary>
            /// Returns the direction vector from a pivot to the base point.
            /// </summary>
            public Vector3 GetDirection(Point p)
            {
                return data.ContainsKey(p) ? data[p].Item1 : Vector3.zero;
            }

            /// <summary>
            /// Returns the stored distance from the pivot to the base point.
            /// </summary>
            public float GetDistance(Point p)
            {
                return data.ContainsKey(p) ? data[p].Item2 : 0f;
            }

            /// <summary>
            /// Move the base point according to stored ratio from all pivots (used to reconstruct).
            /// </summary>
            public void RestorePositionFromPivots()
            {
                if (_pivots.Count == 0) return;

                Vector3 average = Vector3.zero;

                foreach (var kvp in data)
                {
                    Point pivot = kvp.Key;
                    Vector3 dir = kvp.Value.Item1;
                    float dis = kvp.Value.Item2;

                    average += pivot.Position + dir * dis;
                }

                _point.MoveToPosition(average / _pivots.Count);
            }

            /// <summary>
            /// Recalculates ratio data if a pivot has moved.
            /// </summary>
            public void NotifyPivotMoved(Point movedPivot)
            {
                if (_pivots.Contains(movedPivot))
                {
                    UpdatePivot(movedPivot);
                }
            }

            /// <summary>
            /// Clears all pivots and ratio data.
            /// </summary>
            public void Clear()
            {
                _pivots.Clear();
                data.Clear();
            }
            
            // ToString
            
            public override string ToString()
            {
                string result = $"RatioCalculator for Point: {_point.Name}\n";
                result += "Pivot Ratios:\n";

                foreach (var kvp in data)
                {
                    Point pivot = kvp.Key;
                    Vector3 direction = kvp.Value.Item1;
                    float distance = kvp.Value.Item2;

                    result += $"- Pivot: {pivot.Name} | Direction: {direction} | Distance: {distance:F3}\n";
                }

                return result;
            }

            
            public void RecalculatePosition()
            {
                if (_pivots == null || _pivots.Count == 0) return;

                Vector3 finalPosition = Vector3.zero;

                foreach (Point pivot in _pivots)
                {
                    if (!data.ContainsKey(pivot)) continue;

                    Vector3 direction = data[pivot].Item1;
                    float distance = data[pivot].Item2;

                    // Pivot's new position + original direction * original distance
                    Vector3 predicted = pivot.Position + direction * distance;
                    finalPosition += predicted;
                }

                // Average all predicted positions
                finalPosition /= _pivots.Count;

                //Debug.LogWarning($"Location {finalPosition}");

                _point.MoveToPosition(finalPosition); // Assuming this also updates .Position
            }

            
            
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
        public PolygonalShape(Vector3 position, string name, Shape parent) : base(position, name, parent)
        {
        }
    }

// Circular base class (Circle, Sphere, etc.)
    public abstract class CircularShape : Shape
    {
        public CircularShape(Vector3 position, string name, Shape parent) : base(position, name, parent)
        {
        }

        public float Radius { get; set; }
    }
}