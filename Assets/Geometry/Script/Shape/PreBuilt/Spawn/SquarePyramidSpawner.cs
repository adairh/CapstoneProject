using System;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Manipulator
{
    public class SquarePyramidSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition
                {
                    Name = "BaseSide",
                    Type = FieldType.Length,
                    IsRequired = true,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "BaseArea" },
                            Compute = input => Mathf.Sqrt(input["BaseArea"])
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
                            InputFields = new List<string>{ "BaseSide", "SlantHeight" },
                            Compute = input =>
                                Mathf.Sqrt(input["SlantHeight"] * input["SlantHeight"] - (input["BaseSide"] / 2f) * (input["BaseSide"] / 2f))
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "SlantHeight",
                    Type = FieldType.Length,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Height", "BaseSide" },
                            Compute = input =>
                                Mathf.Sqrt(input["Height"] * input["Height"] + (input["BaseSide"] / 2f) * (input["BaseSide"] / 2f))
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "BaseArea",
                    Type = FieldType.Area,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "BaseSide" },
                            Compute = input => input["BaseSide"] * input["BaseSide"]
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "Volume",
                    Type = FieldType.Volume,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "BaseSide", "Height" },
                            Compute = input => (input["BaseSide"] * input["BaseSide"] * input["Height"]) / 3f
                        }
                    }
                }
            };
        }

        public List<ShapeData> ComputeShape(Dictionary<string, float> inputs)
        {
            var solver = new FieldSolver(GetFieldDefinitions());
            var result = solver.Solve(inputs);

            if (!(result.ContainsKey("BaseSide") && result.ContainsKey("Height")))
                throw new Exception("Thiếu cạnh đáy hoặc chiều cao.");

            float a = result["BaseSide"];
            float h = result["Height"];
            Transform lookingPoint = CameraController.Instance.target;

            // Center of base
            Vector3 center = lookingPoint.position + new Vector3(0, 0.5f, 0);

            // 4 base corners
            Vector3 A = center + new Vector3(-a / 2, 0, -a / 2);
            Vector3 B = center + new Vector3(a / 2, 0, -a / 2);
            Vector3 C = center + new Vector3(a / 2, 0, a / 2);
            Vector3 D = center + new Vector3(-a / 2, 0, a / 2);

            // Apex above base center
            Vector3 S = center + new Vector3(0, h, 0);

            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();
            string idS = Guid.NewGuid().ToString();
            string idPyramid = Guid.NewGuid().ToString();

            var data = new List<ShapeData>
            {
                // Points
                new() { Id = idA, Type = "Point", Position = A, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idB, Type = "Point", Position = B, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idC, Type = "Point", Position = C, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idD, Type = "Point", Position = D, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idS, Type = "Point", Position = S, Rotation = Quaternion.identity, Scale = Vector3.one },

                // Base edges
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idB } },
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idC } },
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idD } },
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD, idA } },

                // Sides to apex
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idS } },
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idS } },
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idS } },
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD, idS } },

                // Controller shape (for mesh, always-live)
                new()
                {
                    Id = idPyramid,
                    Type = "SquarePyramid",
                    Position = (A + B + C + D + S) / 5f,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new List<string> { idA, idB, idC, idD, idS }
                }
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(data));
            return data;
        }
    }
}
