namespace Manipulator
{
    public class CreateConstraintAction : IUndoableAction
    {
        private Constraint created;
        private readonly ConstraintData data;

        public CreateConstraintAction(ConstraintData data)
        {
            this.data = data;
        }

        public string Name => "CreateConstraint";

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
    }
}