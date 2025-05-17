using System;
using System.Collections.Generic;

namespace Manipulator
{
    public class FieldDefinition
    {
        public Func<Dictionary<string, float>, float> ComputeFromOthers;
        public bool IsRequired;
        public string Name;
        public FieldType Type;
    }
}