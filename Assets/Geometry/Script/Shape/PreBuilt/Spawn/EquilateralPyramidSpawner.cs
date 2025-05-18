using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class EquilateralPyramidSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "BaseSide", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Height", Type = FieldType.Height, IsRequired = true },
                new FieldDefinition {
                    Name = "BaseArea", Type = FieldType.Area, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("BaseSide")
                            ? Mathf.Pow(inputs["BaseSide"], 2) * Mathf.Sqrt(3f) / 4f
                            : throw new Exception()
                },
                new FieldDefinition {
                    Name = "Volume", Type = FieldType.Area, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("BaseSide") && inputs.ContainsKey("Height")
                            ? (1f / 3f) * (Mathf.Pow(inputs["BaseSide"], 2) * Mathf.Sqrt(3f) / 4f) * inputs["Height"]
                            : throw new Exception()
                }
            };
        }

        public ShapeData ComputeShape(Dictionary<string, float> inputs)
        {
            
            float side = inputs["BaseSide"];
            float height = inputs["Height"];
            IPrebuiltDrawer drawer = new SquareDrawer();
            Vector3 start = ManipulationManager.Instance.TrackingPoint;
            Vector3 end = start + new Vector3(side, 0, 0); // mở rộng theo trục X

            drawer.Begin(start);
            drawer.Working(end);
            drawer.End(end);
            
            
            return new ShapeData
            {
                Type = "EquilateralPyramid",
                Position = Vector3.zero,
                Settings = new Dictionary<string, string>
                {
                    { "baseSide", inputs["BaseSide"].ToString() },
                    { "height", inputs["Height"].ToString() }
                }
            };
        }
    }
}
