using System;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Manipulator
{
    public class RhombusSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition
                {
                    Name = "Side",
                    Type = FieldType.Length,
                    IsRequired = true,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule
                        {
                            InputFields = new List<string> { "Diagonal1", "Diagonal2" },
                            Compute = input =>
                            {
                                float d1 = input["Diagonal1"];
                                float d2 = input["Diagonal2"];
                                // d1 = 2a * sin(alpha/2), d2 = 2a * cos(alpha/2) for rhombus
                                // With only diagonals, assume square for simplicity
                                return Mathf.Sqrt((d1 * d1 + d2 * d2) / 4f);
                            }
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "Diagonal1",
                    Type = FieldType.Length,
                    IsRequired = true,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule
                        {
                            InputFields = new List<string> { "Side", "Diagonal2" },
                            Compute = input =>
                            {
                                float a = input["Side"];
                                float d2 = input["Diagonal2"];
                                // d1^2 + d2^2 = 4a^2
                                return Mathf.Sqrt(4f * a * a - d2 * d2);
                            }
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "Diagonal2",
                    Type = FieldType.Length,
                    IsRequired = true,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule
                        {
                            InputFields = new List<string> { "Side", "Diagonal1" },
                            Compute = input =>
                            {
                                float a = input["Side"];
                                float d1 = input["Diagonal1"];
                                return Mathf.Sqrt(4f * a * a - d1 * d1);
                            }
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
                        new ComputeRule
                        {
                            InputFields = new List<string> { "Diagonal1", "Diagonal2" },
                            Compute = input => 0.5f * input["Diagonal1"] * input["Diagonal2"]
                        }
                    }
                }
            };
        }

        public List<ShapeData> ComputeShape(Dictionary<string, float> inputs)
        {
            var solver = new FieldSolver(GetFieldDefinitions());
            var result = solver.Solve(inputs);

            // Required: Side, Diagonal1, Diagonal2
            if (!result.ContainsKey("Side") || !result.ContainsKey("Diagonal1") || !result.ContainsKey("Diagonal2"))
                throw new Exception("Thiếu dữ liệu cần thiết để dựng hình thoi.");

            float a = result["Side"];
            float d1 = result["Diagonal1"];
            float d2 = result["Diagonal2"];

            // We'll center the rhombus at CameraController.Instance.target.position + (0, 0.5, 0)
            Transform lookingPoint = CameraController.Instance.target;
            Vector3 center = lookingPoint.position + new Vector3(0, 0.5f, 0);

            // Four points of the rhombus in XZ plane:
            Vector3 A = center + new Vector3(d1 / 2f, 0, 0);
            Vector3 C = center - new Vector3(d1 / 2f, 0, 0);
            Vector3 B = center + new Vector3(0, 0, d2 / 2f);
            Vector3 D = center - new Vector3(0, 0, d2 / 2f);

            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();
            string idRhombus = Guid.NewGuid().ToString();

            var data = new List<ShapeData>
            {
                // Points
                new() { Id = idA, Type = "Point", Position = A, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idB, Type = "Point", Position = B, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idC, Type = "Point", Position = C, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idD, Type = "Point", Position = D, Rotation = Quaternion.identity, Scale = Vector3.one },

                // Segments
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idB } },
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idC } },
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idD } },
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD, idA } },

                // Rhombus controller
                new() {
                    Id = idRhombus,
                    Type = "Rhombus",
                    Position = center,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new List<string> { idA, idB, idC, idD }
                }
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(data));
            return data;
        }
    }
}
