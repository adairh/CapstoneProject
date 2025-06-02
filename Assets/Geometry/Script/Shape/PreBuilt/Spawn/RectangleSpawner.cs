using System;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Manipulator
{
    public class RectangleSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition
                {
                    Name = "Width",
                    Type = FieldType.Length,
                    IsRequired = true,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Area", "Height" },
                            Compute = input => input["Area"] / input["Height"]
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
                            InputFields = new List<string>{ "Area", "Width" },
                            Compute = input => input["Area"] / input["Width"]
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
                            InputFields = new List<string>{ "Width", "Height" },
                            Compute = input => input["Width"] * input["Height"]
                        }
                    }
                },
                new FieldDefinition
                {
                    Name = "Diagonal",
                    Type = FieldType.Length,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Width", "Height" },
                            Compute = input => Mathf.Sqrt(Mathf.Pow(input["Width"], 2) + Mathf.Pow(input["Height"], 2))
                        }
                    }
                }
            };
        }

        public List<ShapeData> ComputeShape(Dictionary<string, float> inputs)
        {
            var solver = new FieldSolver(GetFieldDefinitions());
            var result = solver.Solve(inputs);

            if (!result.ContainsKey("Width") || !result.ContainsKey("Height"))
                throw new Exception("Thiếu chiều dài hoặc chiều rộng.");

            float w = result["Width"];
            float h = result["Height"];
            Transform lookingPoint = CameraController.Instance.target;

            Vector3 A = (lookingPoint.position + new Vector3(0, 0.5f, 0)) - new Vector3(w/2, 0, h/2);
            Vector3 B = A + new Vector3(w, 0, 0);
            Vector3 C = A + new Vector3(w, 0, h);
            Vector3 D = A + new Vector3(0, 0, h);

            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();
            string idRectangle = Guid.NewGuid().ToString();

            var data = new List<ShapeData>
            {
                // Points
                new() {Id = idA, Type = "Point", Position = A, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() {Id = idB, Type = "Point", Position = B, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() {Id = idC, Type = "Point", Position = C, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() {Id = idD, Type = "Point", Position = D, Rotation = Quaternion.identity, Scale = Vector3.one },

                // Segments
                new() {Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idB } },
                new() {Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idC } },
                new() {Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idD } },
                new() {Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD, idA } },

                // Rectangle controller
                new() {
                    Id = idRectangle,
                    Type = "Rectangle",
                    Position = (A + B + C + D) / 4f,
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
