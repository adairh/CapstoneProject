using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Manipulator
{
    public class Shape : NetworkBehaviour
    {
        [SerializeField] public string ShapeId;
        [SerializeField] private string shapeType;

        protected readonly List<Point> pivotPoints = new();
        protected readonly List<ISetting> settings = new();
        public string ShapeType => shapeType;
        public ShapeData Data { get; protected set; }
        public IReadOnlyList<Point> PivotPoints => pivotPoints;
        public IReadOnlyList<ISetting> Settings => settings;

        public event Action<Shape> OnChanged;

        // MATERIALS
        public Material DefaultMat { get; set; }
        public Material MeshMat { get; set; } 
        
        public MaterialType CurrentColorType = MaterialType.Default;



        public virtual IEnumerable<Shape> GetDependentShapesForDelete()
        {
            yield return this; // default: chỉ chính nó
        }

        #region INIT

        protected virtual void Awake()
        {
            DefaultMat = new Material(MaterialLibrary.Get(MaterialType.Default));
            
            //MeshMat = MaterialLibrary.GetPolygonMat(); // default polygon mesh material, can override in child
        }

        public virtual List<ISetting> GetSettings()
        {
            return new List<ISetting>
            {
                new PositionSetting(transform.position, this),
                new ColorSetting(MaterialType.Default, this),
                new VisibilitySetting(true, this)
            };
        }

        public virtual void Initialize(ShapeData data)
        {
            ShapeId = data.Id;
            shapeType = data.Type;
            Data = data;
            ApplyDataToTransform(data);
            ShapeStorage.Register(this);

            // PATCH: Always set name from LogicalName
            if (!string.IsNullOrEmpty(data.LogicalName))
                name = data.LogicalName;
        }

        public virtual void InitializeNew(string type, Vector3 position, string lgcName = "")
        {
            ShapeId = Guid.NewGuid().ToString();
            shapeType = type;
            Data = new ShapeData
            {
                Id = ShapeId,
                LogicalName = lgcName,
                Type = type,
                Position = position,
                Rotation = Quaternion.identity,
                Scale = Vector3.one,
                ConnectedPoints = new List<string>(),
                Settings = new Dictionary<string, string>()
            };

            // PATCH: Do not generate label here, it must be provided from the Spawner/Action

            gameObject.AddComponent<HoverableShape>().SetShape(this);
            gameObject.AddComponent<SelectableShape>().SetShape(this);
            gameObject.AddComponent<ShapeClickHandler>().SetShape(this);
            gameObject.AddComponent<DraggableShape>().SetShape(this);

            // PATCH: Do not generate label here!

            ApplyDataToTransform(Data);
            ShapeStorage.Register(this);
        }

        protected virtual void OnDestroy()
        {
            ShapeStorage.Unregister(this);
        }

        public virtual void SetRaycastIgnore(bool ignore)
        {
            var layer = ignore ? 2 : 0;
            gameObject.layer = layer;
            foreach (Transform child in transform)
                if (child != null)
                    child.gameObject.layer = layer;
        }

        public virtual void Dispose()
        {
            ShapeStorage.Unregister(this);
            if (gameObject != null)
                Destroy(gameObject);
        }

        #endregion

        #region TRANSFORM SYNC

        protected virtual void ApplyDataToTransform(ShapeData data)
        {
            ShapeId = data.Id;
            transform.position = data.Position;
            transform.rotation = data.Rotation;
            transform.localScale = data.Scale;
        }

        protected virtual void UpdateDataFromTransform()
        {
            Data.Position = transform.position;
            Data.Rotation = transform.rotation;
            Data.Scale = transform.localScale;
        }

        #endregion

        #region PIVOT

        public void AddPivot(Point p)
        {
            if (!pivotPoints.Contains(p))
            {
                pivotPoints.Add(p);
                p.OnChanged += pt => OnPivotChanged(pt);
            }
        }

        public void RemovePivot(Point p)
        {
            if (pivotPoints.Remove(p))
                p.OnChanged -= pt => OnPivotChanged(pt);
        }

        protected virtual void OnPivotChanged(Point pt)
        {
            NotifyChanged();
        }

        public void MoveToPosition(Vector3 newPos, bool silent = false)
        {
            MoveTo(newPos, silent);
        }

        #endregion

        #region SETTINGS

        public void UpdateSetting(ISetting setting)
        {
            for (var i = 0; i < settings.Count; i++)
                if (settings[i].GetType() == setting.GetType())
                {
                    settings[i] = setting;
                    ApplySetting(setting);
                    NotifyChanged();
                    return;
                }
            settings.Add(setting);
            ApplySetting(setting);
            NotifyChanged();
        }

        protected virtual void ApplySetting(ISetting setting) { }

        public void AppendSettings(params ISetting[] newSettings)
        {
            settings.AddRange(newSettings);
            NotifyChanged();
        }

        #endregion

        #region SERIALIZATION

        public virtual ShapeData Serialize()
        {
            UpdateDataFromTransform();
            return Data;
        }

        public virtual void Deserialize(ShapeData data)
        {
            Data = data;
            ApplyDataToTransform(data);

            // PATCH: Always set name from LogicalName on load
            if (!string.IsNullOrEmpty(data.LogicalName))
                name = data.LogicalName;
        }

        #endregion

        #region MISC

        public virtual void UpdateHitbox() { }
        public virtual void CompleteDraw() { }

        public virtual void NotifyChanged(bool silent = false)
        {
            if (!silent)
                OnChanged?.Invoke(this);
        }

        public virtual void DestroyShape()
        {
            ShapeStorage.Unregister(this);
            Destroy(gameObject);
        }

        public bool isInternalMove;

        public virtual void MoveTo(Vector3 newPosition, bool silent = false, bool queue = true)
        {
            Debug.LogError($"[Shape Move To] {newPosition}");
            if (transform.position == newPosition) return;

            if (!silent && !isInternalMove && !UndoRedoManager.SuppressRecording)
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(
                    new MoveShapeAction(ShapeId, transform.position, newPosition), queue
                );

            isInternalMove = true;
            transform.position = newPosition;
            isInternalMove = false;

            if (!silent)
                NotifyChanged();
        }

        #endregion
    }
}
