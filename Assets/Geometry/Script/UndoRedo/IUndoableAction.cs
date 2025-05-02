namespace Manipulator
{
    public interface IUndoableAction
    {
        void Execute();
        void Undo(); 
    }


}