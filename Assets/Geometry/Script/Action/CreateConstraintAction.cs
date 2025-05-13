
using System;
using UnityEngine;

namespace Manipulator
{
    public class CreateConstraintAction : IUndoableAction
    {
        private ConstraintData data;
        private Constraint created;

        public CreateConstraintAction(ConstraintData data)
        {
            this.data = data;
        }

        public void Redo()
        {
            created = ConstraintFactory.CreateFromData(data);
        }

        public void Undo()
        {
            if (created != null)
            {
                ConstraintFactory.Delete(created);
                created = null;
            }
        }

        public string Name => "CreateConstraint";
    }
}
