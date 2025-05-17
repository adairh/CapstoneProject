using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class GenericPyramidSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "BaseArea", Type = FieldType.Area, IsRequired = true },
                new FieldDefinition { Name = "Height", Type = FieldType.Height, IsRequired = true },
                new FieldDefinition {
                    Name = "Volume", Type = FieldType.Area, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("BaseArea") && inputs.ContainsKey("Height")
                            ? (1f / 3f) * inputs["BaseArea"] * inputs["Height"] : throw new Exception()
                }
            };
        }

        public ShapeData ComputeShape(Dictionary<string, float> inputs)
        {
            return new ShapeData
            {
                Type = "GenericPyramid",
                Position = Vector3.zero,
                Settings = new Dictionary<string, string>
                {
                    { "baseArea", inputs["BaseArea"].ToString() },
                    { "height", inputs["Height"].ToString() }
                }
            };
        }
    }
}
