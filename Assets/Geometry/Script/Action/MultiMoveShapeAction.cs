using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class MultiMoveShapeAction : IUndoableAction
    {
        private readonly List<MoveShapeAction> moveActions = new();

        public MultiMoveShapeAction(List<(string id, Vector3 from, Vector3 to)> moves)
        {
            foreach (var (id, from, to) in moves)
                moveActions.Add(new MoveShapeAction(id, from, to));
        }

        public IEnumerable<MoveShapeAction> GetSubActions() => moveActions;

        public void Undo()
        {
            foreach (var move in moveActions)
                move.Undo();
        }

        public void Redo()
        {
            foreach (var move in moveActions)
                move.Redo();
        }
    }



}