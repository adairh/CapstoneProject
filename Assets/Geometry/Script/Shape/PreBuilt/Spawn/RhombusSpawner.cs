using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class RhombusSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "Diagonal1", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Diagonal2", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition {
                    Name = "Area", Type = FieldType.Area, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Diagonal1") && inputs.ContainsKey("Diagonal2")
                            ? 0.5f * inputs["Diagonal1"] * inputs["Diagonal2"] : throw new Exception()
                }
            };
        }

        public ShapeData ComputeShape(Dictionary<string, float> inputs)
        {
            return new ShapeData
            {
                Type = "Rhombus",
                Position = Vector3.zero,
                Settings = new Dictionary<string, string>
                {
                    { "d1", inputs["Diagonal1"].ToString() },
                    { "d2", inputs["Diagonal2"].ToString() }
                }
            };
        }
    }
}
