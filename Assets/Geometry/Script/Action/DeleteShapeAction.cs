namespace Manipulator
{
    public class DeleteShapeAction : IUndoableAction
    {
        private readonly ShapeData shapeData;
        private Shape shape;

        public DeleteShapeAction(Shape shape)
        {
            this.shape = shape;
            shapeData = shape.Serialize();
        }

        public void Undo()
        {
            if (shape == null)
            {
                shape = ShapeFactory.CreateFromData(shapeData);
                shape.ShapeId = shapeData.Id;
                shape.Deserialize(shapeData);
                ShapeStorage.Register(shape);
            }
        }

        public void Redo()
        {
            if (shape != null)
            {
                shape.DestroyShape();
                shape = null;
            }
        }

        public Shape GetShape()
        {
            return shape;
        }
    }
}