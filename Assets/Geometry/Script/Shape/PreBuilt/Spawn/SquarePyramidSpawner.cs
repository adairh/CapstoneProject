using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class SquarePyramidSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "BaseSide", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Height", Type = FieldType.Height, IsRequired = true },
                new FieldDefinition {
                    Name = "Volume", Type = FieldType.Area, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("BaseSide") && inputs.ContainsKey("Height")
                            ? (1f / 3f) * Mathf.Pow(inputs["BaseSide"], 2) * inputs["Height"] : throw new Exception()
                }
            };
        }

        public ShapeData ComputeShape(Dictionary<string, float> inputs)
        {
            return new ShapeData
            {
                Type = "SquarePyramid",
                Position = Vector3.zero,
                Settings = new Dictionary<string, string>
                {
                    { "base", inputs["BaseSide"].ToString() },
                    { "height", inputs["Height"].ToString() }
                }
            };
        }
    }
}
