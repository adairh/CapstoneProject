
using System.Collections.Generic;
using System.Linq;

namespace Manipulator
{
    public class FieldDependencyGraph
    {
        private List<FieldDefinition> allFields;

        public FieldDependencyGraph(List<FieldDefinition> fields)
        {
            allFields = fields;
        }

        public List<string> ComputeOrder(List<string> knownFields)
        {
            var queue = new Queue<string>(knownFields);
            var visited = new HashSet<string>(knownFields);
            var result = new List<string>(knownFields);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                foreach (var f in allFields)
                {
                    if (visited.Contains(f.Name)) continue;

                    bool hasResolvableRule = f.ComputeRules.Any(rule =>
                        rule.InputFields.All(dep => visited.Contains(dep))
                    );

                    if (hasResolvableRule)
                    {
                        visited.Add(f.Name);
                        queue.Enqueue(f.Name);
                        result.Add(f.Name);
                    }
                }
            }

            return result;
        }

        public List<(string OutputField, List<string> Inputs)> GetResolvedDependencies(List<string> knownFields)
        {
            var dependencies = new List<(string, List<string>)>();
            var visited = new HashSet<string>(knownFields);

            bool changed;
            do
            {
                changed = false;

                foreach (var f in allFields)
                {
                    if (visited.Contains(f.Name)) continue;

                    var rule = f.ComputeRules.FirstOrDefault(r => r.InputFields.All(dep => visited.Contains(dep)));
                    if (rule != null)
                    {
                        dependencies.Add((f.Name, rule.InputFields));
                        visited.Add(f.Name);
                        changed = true;
                    }
                }

            } while (changed);

            return dependencies;
        }
    }
}
