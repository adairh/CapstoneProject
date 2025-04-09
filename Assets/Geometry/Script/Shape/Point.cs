using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class Point : Shape, IDrawable2D
    {
        private int pointNO;
        private SphereCollider collider;
        private Constraint constraint = new FixedPointConstraint(); // Composition
        private HashSet<Shape> attachedShapes = new HashSet<Shape>(); 

        public Point(Vector3 position) : this(position, null)
        {
        }

        public Point(Vector3 position, Shape parent) : base(position, "Pivot " + AlphabetCounter.Next(), parent)
        {
            this.pointNO = AlphabetCounter.CurrentValue();
            SetupGameObject();
            //AttachProcess();
        }


        public void AttachProcess()
        {
            ManipulationManager mm = ManipulationManager.Instance;
            Shape shape = mm.GetPinnedShape();
            if (shape != null)
            {
                if (shape is not Point)
                {
                    /*while (shape.Parent != null)
                    {
                        shape = shape.Parent;
                    }*/
                    //AttachToShape(shape);
                    //CÁI CHỖ NÀY TÍNH TOÁN RATIO VÀ VECTOR ĐỂ AFFECT CONSTRAINT NHỮNG POINT TRÊN SEGMENT HAY ĐÂU ĐÓ ...
                    shape.AddDepend(this);
                    //DEBUG here
                    //Debug.Log($"GetDependData {shape.GetDependData(this).ToString()}");
                } 
            }
        }
        
        
        private void SetupGameObject()
        {
            GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GO.name = Name;
            GO.transform.localScale = Vector3.one * 0.1f; // Small point

            if (Parent != null)
            {
                GO.transform.SetParent(Parent.GO.transform, true); // Preserve world position
                GO.transform.position = Position; // Ensure world position is correct
            }
            else
            {
                GO.transform.position = Position;
            }


            // Register point as a constraint
            ConstraintManager.Instance.RegisterConstraint(constraint);
            constraint.AddShape(this);

            // ✅ Ensure precise SphereCollider
            collider = GO.GetComponent<SphereCollider>();
            if (collider == null) collider = GO.AddComponent<SphereCollider>();
            Drawing();
            UpdateHitbox(); // Ensure hitbox is properly set
        }

        public override void UpdateHitbox()
        {
            if (collider == null) return;

            float worldScale = GO.transform.localScale.x; // Get real world size
            //collider.radius = worldScale; // Set radius correctly
            collider.center = Vector3.zero; // Keep it centered
        }


        public override void Drawing()
        {
            UpdateTransform();
        }

        private void UpdateTransform()
        {
            if (GO == null) return;
            
            GO.transform.position = Position;
            GO.transform.localScale = Vector3.one * 0.1f; // Keep normal size
            
            
            foreach (var shape in attachedShapes)
            {
                //Debug.LogError($"This point {Name} move affect {shape.Name}");
                shape.OnPointMoved(this);
            }
            
            // 🔥 Notify all shapes that depend on this point
            
        }

        protected override void InitializeSettings()
        {
            // LogWarning($"{Name}: InitializeSettings() not implemented.");
        }

        public override GameObject[] Components()
        {
            return new GameObject[] { }; // Use a List instead of an array
        }

        public GameObject GetGameObject()
        {
            return GO;
        }

        public void Draw2D()
        {
            //Debug.Log($"{Name} is being drawn in 2D.");
        }


        public override void CompleteDraw()
        {
            UpdateHitbox();
            base.CompleteDraw();
        }


        public void AttachToShape(Shape shape)
        {
            if (!attachedShapes.Contains(shape))
            {
                attachedShapes.Add(shape);
            }
        }
 
 
    }
}