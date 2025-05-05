using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class Shape : MonoBehaviour
    {
        public string ShapeId { get; protected set; }

        [SerializeField] private string shapeType;
        public string ShapeType => shapeType;

        public ShapeData Data { get; protected set; }

        protected readonly List<Point> pivotPoints = new();
        public IReadOnlyList<Point> PivotPoints => pivotPoints;

        protected readonly List<ISetting> settings = new();
        public IReadOnlyList<ISetting> Settings => settings;

        public event Action<Shape> OnChanged;

        #region INIT

        protected virtual void Awake()
        {
            // override if needed
        }

        public virtual void Initialize(ShapeData data)
        {
            ShapeId = data.Id;
            shapeType = data.Type;
            Data = data;
            ApplyDataToTransform(data);
            ShapeStorage.Register(this);
        }

        public virtual void InitializeNew(string type, Vector3 position)
        {
            ShapeId = Guid.NewGuid().ToString();
            shapeType = type;
            Data = new ShapeData
            {
                Id = ShapeId,
                Type = type,
                Position = position,
                Rotation = Quaternion.identity,
                Scale = Vector3.one,
                ConnectedPoints = new List<string>(),
                Settings = new Dictionary<string, string>()
            };

            ApplyDataToTransform(Data);
            ShapeStorage.Register(this);
        }

        protected virtual void OnDestroy()
        {
            ShapeStorage.Unregister(this);
        }

        
        #endregion

        #region TRANSFORM SYNC

        protected virtual void ApplyDataToTransform(ShapeData data)
        {
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
                p.OnChanged += pt => OnPivotChanged((Point)pt);
            }
        }

        public void RemovePivot(Point p)
        {
            if (pivotPoints.Remove(p))
                p.OnChanged -= pt => OnPivotChanged((Point)pt);
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
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].GetType() == setting.GetType())
                {
                    settings[i] = setting;
                    ApplySetting(setting);
                    NotifyChanged();
                    return;
                }
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

        public virtual void MoveTo(Vector3 newPos, bool silent = false)
        {
            transform.position = newPos;
            NotifyChanged(silent);
        }

        #endregion
    }
}
