using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class EquilateralTriangleSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "Side", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition {
                    Name = "Area", Type = FieldType.Area, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Side") ? (Mathf.Sqrt(3f) / 4f) * Mathf.Pow(inputs["Side"], 2) : throw new Exception()
                },
                new FieldDefinition {
                    Name = "Height", Type = FieldType.Height, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Side") ? (Mathf.Sqrt(3f) / 2f) * inputs["Side"] : throw new Exception()
                }
            };
        }

        public ShapeData ComputeShape(Dictionary<string, float> inputs)
        {
            
            float side = inputs["Side"];
            IPrebuiltDrawer drawer = new SquareDrawer();
            Vector3 start = ManipulationManager.Instance.TrackingPoint;
            Vector3 end = start + new Vector3(side, 0, 0); // mở rộng theo trục X

            drawer.Begin(start);
            drawer.Working(end);
            drawer.End(end);
            
            
            return new ShapeData
            {
                Type = "EquilateralTriangle",
                Position = Vector3.zero,
                Settings = new Dictionary<string, string>
                {
                    { "side", inputs["Side"].ToString() }
                }
            };
        }
    }
}
