using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class SquarePrismSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "Side", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Height", Type = FieldType.Height, IsRequired = true },
                new FieldDefinition {
                    Name = "Volume", Type = FieldType.Area, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Side") && inputs.ContainsKey("Height")
                            ? Mathf.Pow(inputs["Side"], 2) * inputs["Height"] : throw new Exception()
                }
            };
        }

        public ShapeData ComputeShape(Dictionary<string, float> inputs)
        {
            float side = inputs["Side"];
            float height = inputs["Height"];
            IPrebuiltDrawer drawer = new SquareDrawer();
            Vector3 start = ManipulationManager.Instance.TrackingPoint;
            Vector3 end = start + new Vector3(side, 0, 0); // mở rộng theo trục X

            drawer.Begin(start);
            drawer.Working(end);
            drawer.End(end);
            
            return new ShapeData
            {
                Type = "SquarePrism",
                Position = Vector3.zero,
                Settings = new Dictionary<string, string>
                {
                    { "side", inputs["Side"].ToString() },
                    { "height", inputs["Height"].ToString() }
                }
            };
        }
    }
}
