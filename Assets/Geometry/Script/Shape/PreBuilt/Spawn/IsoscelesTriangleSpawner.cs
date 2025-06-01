
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
                new FieldDefinition { Name = "Base", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Side", Type = FieldType.Length, IsRequired = true },

                new FieldDefinition
                {
                    Name = "Height",
                    Type = FieldType.Length,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Side", "Base" },
                            Compute = input => Mathf.Sqrt(Mathf.Pow(input["Side"], 2) - Mathf.Pow(input["Base"] / 2f, 2))
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
                },
                new FieldDefinition
                {
                    Name = "VertexAngle",
                    Type = FieldType.Angle,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "Side", "Base" },
                            Compute = input =>
                            {
                                float s = input["Side"], b = input["Base"];
                                return 2 * Mathf.Acos(b / (2 * s)) * Mathf.Rad2Deg;
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

            if (!(result.ContainsKey("Base") && result.ContainsKey("Side")))
                throw new Exception("Thiếu cạnh đáy hoặc cạnh bên.");

            float baseLen = result["Base"];
            float sideLen = result["Side"];
            float height = result.ContainsKey("Height") ? result["Height"] : Mathf.Sqrt(sideLen * sideLen - Mathf.Pow(baseLen / 2f, 2));

            
            Transform lookingPoint = CameraController.Instance.target;

            Vector3 A = lookingPoint.position - new Vector3(baseLen / 2, 0, sideLen / 2);
            Vector3 B = A + new Vector3(baseLen, 0, 0);
            Vector3 C = A + new Vector3(baseLen / 2f, 0, height);

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
