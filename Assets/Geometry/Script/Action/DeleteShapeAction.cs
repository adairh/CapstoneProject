using UnityEngine;

namespace Manipulator
{
    public class DeleteShapeAction : IUndoableAction
    {
        private readonly Shape shape;

        public DeleteShapeAction(Shape shape)
        {
            this.shape = shape;
        }

        public void Undo()
        {
            shape.gameObject.SetActive(true);
            ShapeStorage.Register(shape);
        }

        public void Redo()
        {
            shape.gameObject.SetActive(false);
            ShapeStorage.Unregister(shape);
        }

        public Shape GetShape() => shape;
    }
}