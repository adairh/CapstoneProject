using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class RightTriangleSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "Base", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Height", Type = FieldType.Height, IsRequired = true },
                new FieldDefinition {
                    Name = "Area", Type = FieldType.Area, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Base") && inputs.ContainsKey("Height")
                            ? 0.5f * inputs["Base"] * inputs["Height"] : throw new Exception()
                }
            };
        }

        public ShapeData ComputeShape(Dictionary<string, float> inputs)
        {
            return new ShapeData
            {
                Type = "RightTriangle",
                Position = Vector3.zero,
                Settings = new Dictionary<string, string>
                {
                    { "base", inputs["Base"].ToString() },
                    { "height", inputs["Height"].ToString() }
                }
            };
        }
    }
}
