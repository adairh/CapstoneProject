namespace Manipulator
{
    public class CreateShapeAction : IUndoableAction
    {
        public Shape Shape { get; }
        public ShapeData Data { get; }

        public CreateShapeAction(Shape shape)
        {
            Shape = shape;
            Data = shape.Serialize();
        }

        public void Undo()
        {
            Shape.DestroyShape();
        }

        public void Redo()
        {
            NetworkShapeSpawner.Instance.CreateShapeNetworked(Data, out Shape s);
        }
    }
}