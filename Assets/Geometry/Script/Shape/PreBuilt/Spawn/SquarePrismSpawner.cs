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
                new FieldDefinition { Name = "Width", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Depth", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Height", Type = FieldType.Length, IsRequired = true },

                new FieldDefinition
                {
                    Name = "Volume",
                    Type = FieldType.Volume,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{"Width", "Depth", "Height"},
                            Compute = input => input["Width"] * input["Depth"] * input["Height"]
                        }
                    }
                }
            };
        }

        public List<ShapeData> ComputeShape(Dictionary<string, float> inputs)
        {
            var solver = new FieldSolver(GetFieldDefinitions());
            var result = solver.Solve(inputs);

            if (!(result.ContainsKey("Width") && result.ContainsKey("Depth") && result.ContainsKey("Height")))
                throw new Exception("Thiếu kích thước khối.");

            float w = result["Width"];
            float d = result["Depth"];
            float h = result["Height"];

            Transform lookingPoint = CameraController.Instance.target;

            // Bottom face (A B C D, counterclockwise)
            Vector3 A = (lookingPoint.position + new Vector3(0, 0.5f, 0)) - new Vector3(w / 2, 0, d / 2);
            Vector3 B = A + new Vector3(w, 0, 0);
            Vector3 C = B + new Vector3(0, 0, d);
            Vector3 D = A + new Vector3(0, 0, d);

            // Top face
            Vector3 A2 = A + new Vector3(0, h, 0);
            Vector3 B2 = B + new Vector3(0, h, 0);
            Vector3 C2 = C + new Vector3(0, h, 0);
            Vector3 D2 = D + new Vector3(0, h, 0);

            // Generate unique ids
            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();
            string idA2 = Guid.NewGuid().ToString();
            string idB2 = Guid.NewGuid().ToString();
            string idC2 = Guid.NewGuid().ToString();
            string idD2 = Guid.NewGuid().ToString();
            string idPrism = Guid.NewGuid().ToString();

            var data = new List<ShapeData>
            {
                // Points
                new() { Id = idA, Type = "Point", Position = A, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idB, Type = "Point", Position = B, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idC, Type = "Point", Position = C, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idD, Type = "Point", Position = D, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idA2, Type = "Point", Position = A2, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idB2, Type = "Point", Position = B2, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idC2, Type = "Point", Position = C2, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idD2, Type = "Point", Position = D2, Rotation = Quaternion.identity, Scale = Vector3.one },

                // Segments (bottom)
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idB }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idC }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idD }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD, idA }},

                // Segments (top)
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA2, idB2 }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB2, idC2 }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC2, idD2 }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD2, idA2 }},

                // Segments (vertical)
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idA2 }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idB2 }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idC2 }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD, idD2 }},

                // Controller
                new()
                {
                    Id = idPrism,
                    Type = "SquarePrism",
                    Position = (A + B + C + D + A2 + B2 + C2 + D2) / 8f,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new List<string> { idA, idB, idC, idD, idA2, idB2, idC2, idD2 }
                }
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(data));
            return data;
        }
    }
}
