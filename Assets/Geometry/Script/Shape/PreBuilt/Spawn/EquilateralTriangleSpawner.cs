using System;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Manipulator
{
    public class EquilateralTriangleSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "Side", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition
                {
                    Name = "Height",
                    Type = FieldType.Length,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Side" },
                            Compute = input => Mathf.Sqrt(3f) / 2f * input["Side"]
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
                            InputFields = new List<string>{ "Side" },
                            Compute = input => Mathf.Sqrt(3f) / 4f * input["Side"] * input["Side"]
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "InternalAngle",
                    Type = FieldType.Angle,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>(),
                            Compute = input => 60f
                        }
                    }
                }
            };
        }

        public List<ShapeData> ComputeShape(Dictionary<string, float> inputs)
        {
            var solver = new FieldSolver(GetFieldDefinitions());
            var result = solver.Solve(inputs);

            if (!result.ContainsKey("Side"))
                throw new Exception("Thiếu độ dài cạnh.");

            float a = result["Side"];
            Transform lookingPoint = CameraController.Instance.target;

            // Geometry
            Vector3 A = (lookingPoint.position + new Vector3(0, 0.5f, 0)) - new Vector3(a / 2, 0, a / 2);
            Vector3 B = A + new Vector3(a, 0, 0);
            Vector3 C = A + new Vector3(a / 2f, 0, Mathf.Sqrt(3f) / 2f * a);

            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();
            string idSegAB = Guid.NewGuid().ToString();
            string idSegBC = Guid.NewGuid().ToString();
            string idSegCA = Guid.NewGuid().ToString();
            string idTriangle = Guid.NewGuid().ToString();

            var data = new List<ShapeData>
            {
                // Points
                new() { Id = idA, Type = "Point", Position = A, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idB, Type = "Point", Position = B, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idC, Type = "Point", Position = C, Rotation = Quaternion.identity, Scale = Vector3.one },

                // Segments
                new() { Id = idSegAB, Type = "Segment", ConnectedPoints = new List<string>{ idA, idB }},
                new() { Id = idSegBC, Type = "Segment", ConnectedPoints = new List<string>{ idB, idC }},
                new() { Id = idSegCA, Type = "Segment", ConnectedPoints = new List<string>{ idC, idA }},

                // Triangle controller
                new() {
                    Id = idTriangle,
                    Type = "EquilateralTriangle",
                    Position = (A + B + C) / 3f,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new List<string>{ idA, idB, idC }
                }
            };

            // Only shape data is returned; mesh will auto-update from controller on spawn
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(data));
            return data;
        }
    }
}
