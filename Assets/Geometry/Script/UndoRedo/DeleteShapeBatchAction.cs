using System.Collections.Generic;

namespace Manipulator
{  

    public class DeleteShapeBatchAction : IUndoableAction
    {
        readonly List<ShapeData>      _shapeDatas;
        readonly List<ConstraintData> _constraintDatas;

        public DeleteShapeBatchAction(
            IEnumerable<Shape>      shapesToDelete,
            IEnumerable<Constraint> constraintsToDelete)
        {
            _shapeDatas      = new List<ShapeData>();
            _constraintDatas = new List<ConstraintData>();

            // Lưu lại toàn bộ data để undo
            foreach (var s in shapesToDelete)
                _shapeDatas.Add(s.Serialize());
            foreach (var c in constraintsToDelete)
                _constraintDatas.Add(c.Serialize());
        }

        public void Execute()
        {
            // 1) Xóa constraints trước (để khỏi vướng vòng lặp)
            foreach (var cd in _constraintDatas)
            {
                // tìm component Constraint tương ứng và Destroy()
                // ConstraintFactory có thể mở rộng để hỗ trợ xóa
                ConstraintFactory.Delete(cd);
            }

            // 2) Xóa shapes
            foreach (var sd in _shapeDatas)
            {
                var s = ShapeStorage.GetShapeByID(sd.Name);
                s?.Destroy();
            } 

        }

        public void Undo()
        {
            // 1) Tạo lại shapes
            foreach (var sd in _shapeDatas)
                ShapeFactory.Create(sd);

            // 2) Tạo lại constraints
            foreach (var cd in _constraintDatas)
                ConstraintFactory.Create(cd);
        }
    }


}