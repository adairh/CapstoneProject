using System.Collections.Generic;

namespace Manipulator
{
    public static class ShapeSolver
    {
        public static Dictionary<string, float> TrySolve(List<FieldDefinition> fields, Dictionary<string, float> inputs)
        {
            var results = new Dictionary<string, float>(inputs);

            bool changed;
            do
            {
                changed = false;
                foreach (var field in fields)
                {
                    if (!results.ContainsKey(field.Name) && field.ComputeFromOthers != null)
                    {
                        try
                        {
                            float value = field.ComputeFromOthers(results);
                            results[field.Name] = value;
                            changed = true;
                        }
                        catch { }
                    }
                }
            } while (changed);

            return results;
        }
    }
}
