using System;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Manipulator
{
    public class IsoscelesTriangleSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition
                {
                    Name = "Base",
                    Type = FieldType.Length,
                    IsRequired = true,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Area", "Height" },
                            Compute = input => 2f * input["Area"] / input["Height"]
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "Height",
                    Type = FieldType.Length,
                    IsRequired = true,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Area", "Base" },
                            Compute = input => 2f * input["Area"] / input["Base"]
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "Side",
                    Type = FieldType.Length,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Base", "Height" },
                            Compute = input => Mathf.Sqrt(Mathf.Pow(input["Base"]/2f, 2) + Mathf.Pow(input["Height"], 2))
                        }
                    }
                },
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
                }
            };
        }

        public List<ShapeData> ComputeShape(Dictionary<string, float> inputs)
        {
            var solver = new FieldSolver(GetFieldDefinitions());
            var result = solver.Solve(inputs);

            if (!result.ContainsKey("Base") || !result.ContainsKey("Height"))
                throw new Exception("Thiếu đáy hoặc chiều cao.");

            float b = result["Base"];
            float h = result["Height"];

            Transform lookingPoint = CameraController.Instance.target;

            Vector3 A = (lookingPoint.position + new Vector3(0, 0.5f, 0)) - new Vector3(b/2, 0, 0);
            Vector3 B = A + new Vector3(b, 0, 0);
            Vector3 C = A + new Vector3(b/2, 0, h);

            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();
            string idTriangle = Guid.NewGuid().ToString();

            var data = new List<ShapeData>
            {
                // Points
                new() { Id = idA, Type = "Point", Position = A, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idB, Type = "Point", Position = B, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idC, Type = "Point", Position = C, Rotation = Quaternion.identity, Scale = Vector3.one },

                // Segments
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idB } },
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idC } },
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idA } },

                // Triangle controller
                new() {
                    Id = idTriangle,
                    Type = "IsoscelesTriangle",
                    Position = (A + B + C) / 3f,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new List<string> { idA, idB, idC }
                }
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(data));
            return data;
        }
    }
}
