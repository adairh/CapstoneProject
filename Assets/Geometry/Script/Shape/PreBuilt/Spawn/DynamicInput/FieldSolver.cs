
using System;
using System.Collections.Generic;
using System.Linq;

namespace Manipulator
{
    public class FieldSolver
    {
        private readonly List<FieldDefinition> fields;

        public FieldSolver(List<FieldDefinition> fieldDefinitions)
        {
            fields = fieldDefinitions;
        }

        public Dictionary<string, float> Solve(Dictionary<string, float> initialInputs)
        {
            var solved = new Dictionary<string, float>(initialInputs);
            bool changed;

            do
            {
                changed = false;

                foreach (var field in fields)
                {
                    if (solved.ContainsKey(field.Name)) continue;

                    var result = field.TryCompute(solved);
                    if (result.HasValue)
                    {
                        solved[field.Name] = result.Value;
                        changed = true;
                    }
                }

            } while (changed);

            return solved;
        }

        public List<string> GetUnsolvableFields(Dictionary<string, float> currentInputs)
        {
            var remaining = new List<string>();

            foreach (var field in fields)
            {
                if (!currentInputs.ContainsKey(field.Name))
                {
                    bool anyRuleWorks = field.ComputeRules.Any(rule => rule.InputFields.All(f => currentInputs.ContainsKey(f)));
                    if (!anyRuleWorks)
                        remaining.Add(field.Name);
                }
            }

            return remaining;
        }
    }
}
