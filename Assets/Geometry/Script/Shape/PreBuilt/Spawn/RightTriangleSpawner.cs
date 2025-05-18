
using System;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Manipulator
{
    public class RightTriangleSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "Base", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Height", Type = FieldType.Length, IsRequired = true },

                new FieldDefinition
                {
                    Name = "Area",
                    Type = FieldType.Area,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Base", "Height" },
                            Compute = input => 0.5f * input["Base"] * input["Height"]
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "Hypotenuse",
                    Type = FieldType.Length,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Base", "Height" },
                            Compute = input => Mathf.Sqrt(input["Base"] * input["Base"] + input["Height"] * input["Height"])
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "RightAngle",
                    Type = FieldType.Angle,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>(),
                            Compute = input => 90f
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "AngleAtBase",
                    Type = FieldType.Angle,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Base", "Height" },
                            Compute = input => Mathf.Atan(input["Height"] / input["Base"]) * Mathf.Rad2Deg
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "AngleAtHeight",
                    Type = FieldType.Angle,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Base", "Height" },
                            Compute = input => Mathf.Atan(input["Base"] / input["Height"]) * Mathf.Rad2Deg
                        }
                    }
                }
            };
        }

        public List<ShapeData> ComputeShape(Dictionary<string, float> inputs)
        {
            var solver = new FieldSolver(GetFieldDefinitions());
            var result = solver.Solve(inputs);

            if (!(result.ContainsKey("Base") && result.ContainsKey("Height")))
                throw new Exception("Thiếu chiều dài cạnh đáy hoặc chiều cao.");

            float b = result["Base"];
            float h = result["Height"];

            Vector3 A = ManipulationManager.Instance.TrackingPoint;
            Vector3 B = A + new Vector3(b, 0, 0);
            Vector3 C = A + new Vector3(0, 0, h);

            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();

            var data = new List<ShapeData>
            {
                new() { Id = idA, Type = "Point", Position = A, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idB, Type = "Point", Position = B, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idC, Type = "Point", Position = C, Rotation = Quaternion.identity, Scale = Vector3.one },

                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idB }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idC }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idA }}
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(data));
            return data;
        }
    }
}
