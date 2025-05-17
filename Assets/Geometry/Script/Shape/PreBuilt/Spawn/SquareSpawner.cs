using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class SquareSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition
                {
                    Name = "Side",
                    Type = FieldType.Length,
                    IsRequired = true
                },
                new FieldDefinition
                {
                    Name = "Area",
                    Type = FieldType.Area,
                    IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Side") ? inputs["Side"] * inputs["Side"] : throw new Exception()
                },
                new FieldDefinition
                {
                    Name = "Perimeter",
                    Type = FieldType.Perimeter,
                    IsRequired = false,
                    ComputeFromOthers = inputs =>
                        inputs.ContainsKey("Side") ? 4 * inputs["Side"] : throw new Exception()
                }
            };
        }

        public ShapeData ComputeShape(Dictionary<string, float> inputs)
        {
            float side = inputs["Side"];
            IPrebuiltDrawer drawer = new SquareDrawer();
            Vector3 start = Vector3.zero;
            Vector3 end = start + new Vector3(side, 0, 0); // mở rộng theo trục X

            drawer.Begin(start);
            drawer.Working(end);
            drawer.End(end);

            
            return new ShapeData
            {
                Id = Guid.NewGuid().ToString(),
                Type = "Square",
                Position = Vector3.zero,
                Settings = new Dictionary<string, string>
                {
                    { "side", side.ToString() }
                }
            };

            
        }
    }
}
