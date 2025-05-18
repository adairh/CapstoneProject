
using System;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Manipulator
{
    public class SquarePrismSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "Side", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Height", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition
                {
                    Name = "Volume",
                    Type = FieldType.Volume,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Side", "Height" },
                            Compute = input => input["Side"] * input["Side"] * input["Height"]
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "SurfaceArea",
                    Type = FieldType.Area,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Side", "Height" },
                            Compute = input =>
                            {
                                float a = input["Side"];
                                float h = input["Height"];
                                return 2 * a * a + 4 * a * h;
                            }
                        }
                    }
                }
            };
        }

        public List<ShapeData> ComputeShape(Dictionary<string, float> inputs)
        {
            var solver = new FieldSolver(GetFieldDefinitions());
            var result = solver.Solve(inputs);
            if (!result.ContainsKey("Side") || !result.ContainsKey("Height"))
                throw new Exception("Thiếu cạnh đáy hoặc chiều cao.");

            float a = result["Side"];
            float h = result["Height"];

            Vector3 A = ManipulationManager.Instance.TrackingPoint;
            Vector3 B = A + new Vector3(a, 0, 0);
            Vector3 C = A + new Vector3(a, 0, a);
            Vector3 D = A + new Vector3(0, 0, a);

            Vector3 A2 = A + new Vector3(0, h, 0);
            Vector3 B2 = B + new Vector3(0, h, 0);
            Vector3 C2 = C + new Vector3(0, h, 0);
            Vector3 D2 = D + new Vector3(0, h, 0);

            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();
            string idA2 = Guid.NewGuid().ToString();
            string idB2 = Guid.NewGuid().ToString();
            string idC2 = Guid.NewGuid().ToString();
            string idD2 = Guid.NewGuid().ToString();

            var data = new List<ShapeData>
            {
                new() { Id = idA, Type = "Point", Position = A, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idB, Type = "Point", Position = B, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idC, Type = "Point", Position = C, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idD, Type = "Point", Position = D, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idA2, Type = "Point", Position = A2, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idB2, Type = "Point", Position = B2, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idC2, Type = "Point", Position = C2, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idD2, Type = "Point", Position = D2, Rotation = Quaternion.identity, Scale = Vector3.one },

                // Bottom base
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idB }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idC }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idD }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD, idA }},

                // Top base
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA2, idB2 }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB2, idC2 }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC2, idD2 }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD2, idA2 }},

                // Vertical edges
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idA2 }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idB2 }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idC2 }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD, idD2 }}
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(data));
            return data;
        }
    }
}
