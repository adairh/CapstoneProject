using System.Collections.Generic;

namespace Manipulator
{
    public interface IShapeSpawner
    {
        List<FieldDefinition> GetFieldDefinitions();
        ShapeData ComputeShape(Dictionary<string, float> inputs);
    }
}