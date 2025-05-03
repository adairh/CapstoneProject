using System.Collections.Generic;
using Manipulator;
using UnityEngine;

namespace Manipulator
{
    interface IConstraintSnapshot
    {
        void Remove();           // gỡ constraint khỏi scene
        void Restore();          // tạo lại và đăng ký constraint với chính xác các tham số cũ
    }
    
    public abstract class Constraint : MonoBehaviour
    {
        private List<Shape> linkedShapes = new List<Shape>();
        public Shape Owner { get; set; }
        
        public HologramLabel Holo { get; set; }

        public void AddShape(Shape shape)
        {
            if (!linkedShapes.Contains(shape))
            {
                linkedShapes.Add(shape);
            }
        }

        public bool HasShape(Shape shape)
        {
            return linkedShapes.Contains(shape);
        }

        public List<Shape> GetLinkedShapes()
        {
            return new List<Shape>(linkedShapes); // Return a copy to avoid modification
        }

        public abstract void ApplyConstraint(Shape movedShape, Vector3 movement);


        public abstract string GetLabelText();
        public abstract ConstraintData Serialize();
    }
}