using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class RegularTetrahedronSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "Edge", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition {
                    Name = "Volume", Type = FieldType.Area, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Edge")
                            ? Mathf.Pow(inputs["Edge"], 3) / (6 * Mathf.Sqrt(2)) : throw new Exception()
                },
                new FieldDefinition {
                    Name = "Height", Type = FieldType.Height, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Edge")
                            ? Mathf.Sqrt(2f / 3f) * inputs["Edge"] : throw new Exception()
                }
            };
        }

        public ShapeData ComputeShape(Dictionary<string, float> inputs)
        {
            return new ShapeData
            {
                Type = "RegularTetrahedron",
                Position = Vector3.zero,
                Settings = new Dictionary<string, string>
                {
                    { "edge", inputs["Edge"].ToString() }
                }
            };
        }
    }
}
