using System.Collections.Generic;

namespace Manipulator
{
    public class DeleteShapeBatchAction : IUndoableAction
    {
        public readonly List<ShapeData> shapeDataList = new();
        public readonly List<Shape> shapes = new();

        public DeleteShapeBatchAction(IEnumerable<Shape> shapesToDelete)
        {
            HashSet<Shape> unique = new();

            foreach (var shape in shapesToDelete)
            foreach (var dep in shape.GetDependentShapesForDelete())
                if (dep != null && unique.Add(dep))
                {
                    shapes.Add(dep);
                    shapeDataList.Add(dep.Serialize());
                }
        }


        public void Undo()
        {
            foreach (var data in shapeDataList)
            {
                var shape = ShapeFactory.CreateFromData(data);
                shape.ShapeId = data.Id;
                shape.Deserialize(data);
                ShapeStorage.Register(shape);
            }
        }

        public void Redo()
        {
            foreach (var shape in shapes) shape.DestroyShape();
            shapes.Clear();
        }
    }
}