using System;
using System.Collections.Generic;
using Unity.Netcode;
using Geometry.Script.Network;
using Manipulator.Data;
using UnityEngine;
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

        public static event Action<string> ShapeAdded;
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
        public event Action<Shape> OnChanged;
        protected void NotifyChange()
        {
            OnChanged?.Invoke(this);
            Parent?.NotifyChange();
        }

        private ShapeNetworkSync syncer;
        private GameObject go;

        protected readonly List<ISetting> settings = new List<ISetting>();
        public readonly List<Point> PivotPoints = new List<Point>();

        public Vector3 Position { get; set; }
        public Color ShapeColor { get; set; }
        public string Name { get; set; }
        public int id { get; set; }
        public Shape Parent { get; set; }
        public bool IsSnappable { get; set; } = true;

        public Material DefaultMaterial { get; set; }
        public Material HighlightMaterial { get; set; }
        public EditableShape EditableShape;

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

        protected virtual void RegisterEvents()
        {
            go.tag = Parent == null ? "Shape" : "Child";
            go.AddComponent<ShapeClickHandler>().SetShape(this);
            go.AddComponent<DraggableShape>().SetShape(this);
            go.AddComponent<HoverableShape>().SetShape(this);
            if (go.CompareTag("Shape"))
                go.AddComponent<SelectableShape>().SetShape(this);
        }

        protected virtual void SetupGameObject() { }

        public void AssignSyncer(ShapeNetworkSync networkSync) => syncer = networkSync;
        public ShapeNetworkSync GetSNS() => syncer;

        public void AdjustToPosition(Vector3 newPos, bool silent = false)
        {
            Vector3 offset = newPos - Position;
            foreach (var p in PivotPoints)
                p.MoveTo(p.Position + offset);

            Position = newPos;
            go.transform.position = newPos;

            if (!silent)
                NotifyChange();
        }

        public virtual void MoveToPosition(Vector3 newPos, bool silent = false)
        {
            Vector3 delta = newPos - Position;
            AdjustToPosition(newPos, silent);
            CompleteSettings();
            Draw();
            UpdateHitbox();
            CompleteDraw();
            syncer?.MoveShape(newPos);
            if (!silent) NotifyChange();

            // Áp constraint cho mọi liên kết
            ConstraintManager.Instance.ApplyConstraints(this, delta);
        }

        public virtual void OnPointMoved(Point movedPoint) { }
        public virtual void OnChildChanged(Shape child)
        {
            if (child is Point pt)
                MovePivots(pt);
        }

        public virtual void MovePivots(Point movedPoint) { NotifyChange(); }
        public virtual void MovePivots(string pointName, Vector3 loc) { NotifyChange(); }

        protected abstract void InitializeSettings();
        public virtual void ApplySettings() { }
        public virtual void OnSettingChanged(ISetting setting) => ApplySettings();

        public void UpdateSettings(ISetting setting) { /* ... */ NotifyChange(); }
        public void AppendSettings(params ISetting[] newSettings) { /* ... */ NotifyChange(); }
        public void ModifySetting<T>(ISetting setting, T value) { /* ... */ NotifyChange(); }

        public abstract void Drawing();
        public abstract void UpdateHitbox();
        public abstract GameObject[] Components();

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

        public virtual void CompleteSettings() { }

        public void SetIgnoreRaycast(bool ignore)
        {
            int layer = ignore ? 2 : 0;
            go.layer = layer;
            foreach (Transform c in go.transform)
                c.gameObject.layer = layer;
        }

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

        public virtual void FullRefresh() { }
        public virtual void BeginSketch(Vector3 vector) { }
        public virtual void UpdateSketch(Vector3 vector) { }
        public virtual void EndSketch(Vector3 vector) { }

        public virtual ShapeData Serialize() => null;
        public virtual void Deserialize(ShapeData data) { }

        public static Quaternion GetAlignedRotation(Camera cam)
        {
            var fwd = cam.transform.forward;
            if (fwd == Vector3.zero) fwd = Vector3.forward;
            return Quaternion.LookRotation(fwd, Vector3.up);
        }

        public virtual void Destroy()
        {
            if (ShapeStorage.GetShapeByID(go.name) != null)
                ShapeStorage.RemoveShape(go.name);
            Object.Destroy(go);
        }
    }

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
    public interface IDrawable2D { void Draw2D(); }
    public interface IDrawable3D { void Draw3D(); }
}
