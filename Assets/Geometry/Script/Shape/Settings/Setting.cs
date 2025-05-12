using UnityEngine;

namespace Manipulator
{
// Base Interface for All Settings
    public interface ISetting
    {
        GameObject CreateUI(Transform parent); // Gắn trực tiếp vào layout
        void LoadFromShape();                 // Load giá trị từ shape
        void ApplyToShape();                  // Apply khi chỉnh xong
    }


// Generic Abstract Class for Settings
    public abstract class Setting<T> : ISetting
    {
        public T Value { get; protected set; }
        public Shape TargetShape { get; set; }
        public GameObject UIInstance { get; protected set; }
        public GameObject Prefab { get; protected set; }

        protected Setting(T initialValue, Shape shape, GameObject prefab)
        {
            Value = initialValue;
            TargetShape = shape;
            Prefab = prefab;
        }

        public abstract GameObject CreateUI(Transform parent);
        public abstract void LoadFromShape();
        public abstract void ApplyToShape();

        public void SetValue(object value)
        {
            if (value is T cast) Value = cast;
        }

        public virtual void SetValue(T value)
        {
            Value = value;
        }
    }

}