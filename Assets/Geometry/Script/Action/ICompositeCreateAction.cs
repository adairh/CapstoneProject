using System.Collections.Generic;

namespace Manipulator
{
    public interface ICompositeCreateAction : IUndoableAction
    {
        List<ShapeData> GetAllShapeData();
        void LinkReferences(Dictionary<string, Shape> createdShapes);
    }

}