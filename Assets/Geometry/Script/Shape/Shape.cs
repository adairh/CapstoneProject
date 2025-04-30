using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Geometry.Script.Network;
using Manipulator.Data;
using Object = UnityEngine.Object;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Manipulator
{
    // Shape storage for lookup and nearest-point snapping
    public static class ShapeStorage
    { 
        private static readonly Dictionary<string, Shape> shapes = new Dictionary<string, Shape>();
        private const int IGNORE_RAYCAST_LAYER = 2;
        
        /// <summary>Được gọi ngay sau khi một shape mới được thêm vào.</summary>
        public static event Action<string> ShapeAdded;

        /// <summary>Được gọi ngay sau khi một shape bị xóa.</summary>
        public static event Action<string> ShapeRemoved;

        public static Shape GetShapeByID(string id)
            => shapes.TryGetValue(id, out var s) ? s : null;

        public static void AddShape(string id, Shape shape)
        {
            shapes[id] = shape;
            ShapeAdded?.Invoke(id);
        }

        public static void RemoveShape(string id)
        {
            if (shapes.Remove(id))
                ShapeRemoved?.Invoke(id);
        }

        public static IEnumerable<Shape> GetAllShapes() => shapes.Values;

        public static Point FindNearestPoint(Vector3 position, float maxSnapDistance = 0.1f)
        {
            Point closest = null;
            float minSqr = maxSnapDistance * maxSnapDistance;
            foreach (var s in shapes.Values)
            {
                if (s is Point pt && pt.IsSnappable && pt.GO.layer != IGNORE_RAYCAST_LAYER)
                {
                    float sqr = (pt.Position - position).sqrMagnitude;
                    if (sqr < minSqr && sqr > 0f)
                    {
                        closest = pt;
                        minSqr = sqr;
                    }
                }
            }
            return closest;
        }
    }

    public abstract class Shape : ISynchronizedShape
    {
        // Event for change notifications
        public event Action<Shape> OnChanged;
        public void NotifyChange()
        {
            OnChanged?.Invoke(this);
            Parent?.NotifyChange();
        }

        // Network syncer reference
        private ShapeNetworkSync syncer;

        // Backing GameObject
        private GameObject go;

        // Settings, pivots, and dependencies
        protected readonly List<ISetting> settings = new List<ISetting>();
        public readonly List<Point> PivotPoints = new List<Point>();

        // Shape properties
        public Vector3 Position { get; set; }
        public Color ShapeColor { get; set; }
        public string Name { get; set; }
        public int id { get; set; }
        public Shape Parent { get; set; }
        public bool IsSnappable { get; set; } = true;

        public Material DefaultMaterial { get; set; }
        public Material HighlightMaterial { get; set; }
        public EditableShape EditableShape;

        // Access to GameObject
        public GameObject GO
        {
            get => go;
            set
            {
                go = value;
                go.name = Name;
                RegisterEvents();
                SetupGameObject();
            }
        }

        // Constructor
        protected Shape(Vector3 position, string baseName, Shape parent)
        {
            Position = position;
            Name = baseName + " " + ObjectCounter.Next();
            id = ObjectCounter.Current();
            Parent = parent;

            ShapeColor = Color.red;
            DefaultMaterial = MaterialLibrary.Get(MaterialType.Default);
            HighlightMaterial = MaterialLibrary.Get(MaterialType.Highlight);

            ShapeStorage.AddShape(Name, this);
            InitializeSettings();
        }

        // Register click and drag handlers
        protected virtual void RegisterEvents()
        {
            go.tag = Parent == null ? "Shape" : "Child";
            go.AddComponent<ShapeClickHandler>().SetShape(this);
            go.AddComponent<DraggableShape>().SetShape(this);
            go.AddComponent<HoverableShape>().SetShape(this);
            
            if (go.CompareTag("Shape"))
                go.AddComponent<SelectableShape>().SetShape(this);
        }

        // Additional setup (override in subclasses)
        protected virtual void SetupGameObject() { }

        // Assign network syncer
        public void AssignSyncer(ShapeNetworkSync networkSync) => syncer = networkSync;
        public ShapeNetworkSync GetSNS() => syncer;

        // Position adjustment without full redraw
        public void AdjustToPosition(Vector3 newPos, bool silent, bool transformGO = true)
        {
            Vector3 offset = newPos - Position;
            foreach (var p in PivotPoints)
                p.MoveTo(p.Position + offset);

            Position = newPos;
            if (transformGO && go != null)
                go.transform.position = newPos;

            if (!silent) NotifyChange();
        }

        // Full move
        public virtual void MoveToPosition(Vector3 newPos, bool silent = false)
        {
            AdjustToPosition(newPos, silent);
            CompleteSettings();
            Draw();
            UpdateHitbox();
            CompleteDraw();
            syncer?.MoveShape(newPos);
            if (!silent) NotifyChange();
        }

        // Called when a pivot point moves
        public virtual void OnPointMoved(Point movedPoint)
        {
            NotifyChange();
        }

        // Subscribe callbacks: child change
        public virtual void OnChildChanged(Shape child)
        {
            if (child is Point pt)
                MovePivots(pt);
        }

        // MovePivots overloads
        public virtual void MovePivots(Point movedPoint)
        {
            NotifyChange();
        }
        public virtual void MovePivots(string pointName, Vector3 loc)
        {
            NotifyChange();
        }

        // Settings initialization and application
        protected abstract void InitializeSettings();
        public virtual void ApplySettings() { }
        public virtual void OnSettingChanged(ISetting setting) => ApplySettings();

        public void UpdateSettings(ISetting setting)
        {
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].GetType() == setting.GetType())
                {
                    settings[i] = setting;
                    OnSettingChanged(setting);
                    NotifyChange();
                    return;
                }
            }
            settings.Add(setting);
            OnSettingChanged(setting);
            NotifyChange();
        }
        public void AppendSettings(params ISetting[] newSettings)
        {
            settings.AddRange(newSettings);
            NotifyChange();
        }
        public void ModifySetting<T>(ISetting setting, T value)
        {
            setting.SetValue(value);
            UpdateSettings(setting);
        }

        // Abstract draw methods
        public abstract void Drawing();
        public abstract void UpdateHitbox();
        public abstract GameObject[] Components();

        // Draw entry
        public void Draw()
        {
            SetIgnoreRaycast(true);
            Drawing();
        }
        public virtual void CompleteDraw()
        {
            PerformDrawing.ResetShape();
            if (go.TryGetComponent<HoverableShape>(out var hs))
                hs.SetComponents();
            SetIgnoreRaycast(false); 
        }

        // Complete settings hook
        public virtual void CompleteSettings() { }

        // Raycast control
        protected const int IGNORE_RAYCAST_LAYER = 2;
        private const int defaultLayer = 0;
        public void SetIgnoreRaycast(bool ignore)
        {
            if (go == null) return;
            int layer = ignore ? IGNORE_RAYCAST_LAYER : defaultLayer;
            go.layer = layer;
            foreach (Transform c in go.transform)
                c.gameObject.layer = layer;
        }

        // Settings and pivots access
        public List<ISetting> GetSettings() => settings;
        public List<Point> GetPivots() => PivotPoints;

        public void AddPivot(Point point)
        {
            if (!PivotPoints.Contains(point))
            {
                PivotPoints.Add(point);
                point.OnChanged += OnChildChanged;
            }
        }
        public void RemovePivot(Point point)
        {
            if (PivotPoints.Remove(point))
                point.OnChanged -= OnChildChanged;
        }
        public bool IsPivot(Point point) => PivotPoints.Contains(point);


        // Refresh and serialization
        public virtual void FullRefresh() { }
        public virtual void BeginSketch(Vector3 vector)
        {
        }

        public virtual void UpdateSketch(Vector3 vector)
        {
        }

        public virtual void EndSketch(Vector3 vector)
        {
        }

        public virtual ShapeData Serialize() => null;
        public virtual void Deserialize(ShapeData data) { }

        // Utility rotation
        public static Quaternion GetAlignedRotation(Camera cam)
        {
            var fwd = cam.transform.forward;
            if (fwd == Vector3.zero) fwd = Vector3.forward;
            return Quaternion.LookRotation(fwd, Vector3.up);
        }

        // Cleanup
        public virtual void Destroy()
        {
            if (ShapeStorage.GetShapeByID(go.name) != null)
                ShapeStorage.RemoveShape(go.name);
            Object.Destroy(go);
        }

        // RatioCalculator nested class
        
    }

    public interface IDrawable2D { void Draw2D(); }
    public interface IDrawable3D { void Draw3D(); }

    public abstract class PolygonalShape : Shape
    {
        protected PolygonalShape(Vector3 position, string name, Shape parent)
            : base(position, name, parent) { }
    }

    public abstract class CircularShape : Shape
    {
        protected CircularShape(Vector3 position, string name, Shape parent)
            : base(position, name, parent) { }
        public float Radius { get; set; }
    }
}
