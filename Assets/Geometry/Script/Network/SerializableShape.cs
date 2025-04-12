using Manipulator;
using UnityEngine;

namespace Geometry.Script.Network
{
    public abstract class SerializableShape : Shape, IShapeSerializable
    {
        public abstract string GetShapeType();

        public virtual string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public virtual void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
            Draw();
            CompleteDraw();
        }

        protected SerializableShape(Vector3 position, string name, Shape parent) : base(position, name, parent)
        {
        }
    }

}