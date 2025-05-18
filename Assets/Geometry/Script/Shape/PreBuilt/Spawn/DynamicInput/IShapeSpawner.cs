using System.Collections.Generic;

namespace Manipulator
{
    public interface IShapeSpawner
    {
        List<FieldDefinition> GetFieldDefinitions();
        List<ShapeData> ComputeShape(Dictionary<string, float> inputs);
    }
}