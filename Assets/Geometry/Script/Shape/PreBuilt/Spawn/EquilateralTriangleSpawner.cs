
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

            Vector3 A = (lookingPoint.position + new Vector3(0, 0.5f, 0)) - new Vector3(a/2, 0, a/2);
            Vector3 B = A + new Vector3(a, 0, 0);
            Vector3 C = A + new Vector3(a / 2f, 0, Mathf.Sqrt(3f) / 2f * a);

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
            MeshGenerator.MeshCompute(new []{A, B, C},
                new []{
                    0, 1, 2
                }, new []
                {
                    ShapeStorage.GetById(idA).gameObject.transform, 
                    ShapeStorage.GetById(idB).gameObject.transform, 
                    ShapeStorage.GetById(idC).gameObject.transform,  
                });
            return data;
        }
    }
}
