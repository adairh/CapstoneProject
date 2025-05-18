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
            float side = inputs["BaseArea"];
            float height = inputs["height"];
            IPrebuiltDrawer drawer = new SquareDrawer();
            Vector3 start = ManipulationManager.Instance.TrackingPoint;
            Vector3 end = start + new Vector3(side, 0, 0); // mở rộng theo trục X

            drawer.Begin(start);
            drawer.Working(end);
            drawer.End(end);
            
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
