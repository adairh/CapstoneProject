using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class IsoscelesTriangleSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "Base", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Side", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition {
                    Name = "Height", Type = FieldType.Height, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Side") && inputs.ContainsKey("Base")
                            ? Mathf.Sqrt(inputs["Side"] * inputs["Side"] - (inputs["Base"] * inputs["Base"]) / 4f)
                            : throw new Exception()
                },
                new FieldDefinition {
                    Name = "Area", Type = FieldType.Area, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Base") && inputs.ContainsKey("Side")
                            ? 0.5f * inputs["Base"] * Mathf.Sqrt(inputs["Side"] * inputs["Side"] - (inputs["Base"] * inputs["Base"]) / 4f)
                            : throw new Exception()
                }
            };
        }

        public ShapeData ComputeShape(Dictionary<string, float> inputs)
        {
            return new ShapeData
            {
                Type = "IsoscelesTriangle",
                Position = Vector3.zero,
                Settings = new Dictionary<string, string>
                {
                    { "base", inputs["Base"].ToString() },
                    { "side", inputs["Side"].ToString() }
                }
            };
        }
    }
}
