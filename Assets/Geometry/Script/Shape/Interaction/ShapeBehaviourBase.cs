using UnityEngine;

namespace Manipulator
{
    public abstract class ShapeBehaviourBase : MonoBehaviour
    {
        protected Shape shape;

        public virtual void SetShape(Shape s)
        {
            shape = s;
        }

        public Shape GetShape()
        {
            return shape;
        }
    }
}