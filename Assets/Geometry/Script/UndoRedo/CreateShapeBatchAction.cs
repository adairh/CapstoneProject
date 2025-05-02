using System.Collections.Generic;

namespace Manipulator
{
    public class CreateShapeBatchAction : IUndoableAction
    {
        private readonly List<string> _shapeIds;

        public CreateShapeBatchAction(List<string> shapeIds)
        {
            // Clone list để độc lập với biến ngoài
            _shapeIds = new List<string>(shapeIds);
        }

        public void Execute()
        {
            // nothing: shapes đã được tạo khi FinishDrawing 
        }

        public void Undo()
        {
            // Xoá tất cả những shape này
            foreach (var id in _shapeIds)
            {
                var s = ShapeStorage.GetShapeByID(id);
                if (s != null)
                    s.Destroy();
            }
        }
    }
}