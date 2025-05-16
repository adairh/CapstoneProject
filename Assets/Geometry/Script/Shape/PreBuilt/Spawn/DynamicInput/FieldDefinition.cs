using System;
using System.Collections.Generic;

namespace Manipulator
{
    public class FieldDefinition
    {
        public string Name;
        public FieldType Type;
        public bool IsRequired;
        public Func<Dictionary<string, float>, float> ComputeFromOthers;
    }
}
