namespace Manipulator
{
    public interface IUndoableAction
    {
        void Undo();
        void Redo();
    }


}