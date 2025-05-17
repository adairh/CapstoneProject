using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class RectangleSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "Length", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Width", Type = FieldType.Width, IsRequired = true },
                new FieldDefinition {
                    Name = "Area", Type = FieldType.Area, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Length") && inputs.ContainsKey("Width")
                            ? inputs["Length"] * inputs["Width"] : throw new Exception()
                },
                new FieldDefinition {
                    Name = "Perimeter", Type = FieldType.Perimeter, IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Length") && inputs.ContainsKey("Width")
                            ? 2 * (inputs["Length"] + inputs["Width"]) : throw new Exception()
                }
            };
        }

        public ShapeData ComputeShape(Dictionary<string, float> inputs)
        {
            float side = inputs["Length"];
            IPrebuiltDrawer drawer = new SquareDrawer();
            Vector3 start = ManipulationManager.Instance.TrackingPoint;
            Vector3 end = start + new Vector3(side, 0, 0); // mở rộng theo trục X

            drawer.Begin(start);
            drawer.Working(end);
            drawer.End(end);
            
            return new ShapeData
            {
                Type = "Rectangle",
                Position = Vector3.zero,
                Settings = new Dictionary<string, string>
                {
                    { "length", inputs["Length"].ToString() },
                    { "width", inputs["Width"].ToString() }
                }
            };
        }
    }
}
