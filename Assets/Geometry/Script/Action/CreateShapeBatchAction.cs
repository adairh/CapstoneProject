using System.Collections.Generic;

namespace Manipulator
{
    public class CreateShapeBatchAction : IUndoableAction
    {
        private readonly List<ShapeData> datas = new();
        private readonly List<string> createdIds = new();

        public void Add(ShapeData data) => datas.Add(data);

        public void Redo()
        {
            createdIds.Clear();
            foreach (var data in datas)
            {
                Shape shape = ShapeFactory.CreateFromData(data);
                createdIds.Add(shape.ShapeId);

                // Gọi callback
                if (shape is Point pt)
                    Segment.Drawer.OnStartPointReady(pt);
                else if (shape is Segment seg)
                    Segment.Drawer.OnSegmentReady(seg);
            }
        }

        public void Undo()
        {
            foreach (string id in createdIds)
            {
                var shape = ShapeStorage.GetById(id);
                if (shape != null)
                    shape.DestroyShape();
            }
        }
    }

}