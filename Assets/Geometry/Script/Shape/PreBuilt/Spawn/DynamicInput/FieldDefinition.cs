
using System;
using System.Collections.Generic;
using System.Linq;

namespace Manipulator
{
    public class ComputeRule
    {
        public List<string> InputFields;
        public Func<Dictionary<string, float>, float> Compute;
    }

    public class FieldDefinition
    {
        public string Name;
        public FieldType Type;
        public bool IsRequired;

        // Multi-rule inference support
        public List<ComputeRule> ComputeRules = new();

        // Shortcut: return first applicable rule
        public float? TryCompute(Dictionary<string, float> knownInputs)
        {
            foreach (var rule in ComputeRules)
            {
                if (rule.InputFields.All(f => knownInputs.ContainsKey(f)))
                {
                    try
                    {
                        return rule.Compute(knownInputs);
                    }
                    catch { return null; }
                }
            }

            return null;
        }
        
        public void ComputeFromOthers() {}
    }
 
}
