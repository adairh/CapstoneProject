using System.Collections.Generic;
using System.Linq;
using Geometry.Script.Network;
using Manipulator.Data;

namespace Manipulator
{
    public class DeleteAction : IUndoableAction
    {
        private readonly List<Shape> _shapes;

        public DeleteAction(IEnumerable<Shape> shapesToDelete)
        {
            _shapes = new List<Shape>(shapesToDelete);
        }

        public void Execute()
        {
            // Soft-delete tất cả
            foreach (var s in _shapes)
                s.SoftDelete();
        }

        public void Undo()
        {
            // Restore lại
            foreach (var s in _shapes)
                s.Restore();
        }
    }
}