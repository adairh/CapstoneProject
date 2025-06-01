
using System;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Manipulator
{
    public class EquilateralPyramidSpawner : IShapeSpawner
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
                            Compute = input =>
                            {
                                float a = input["Side"];
                                return Mathf.Sqrt(a * a - (a * a / 2f));
                            }
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
                            InputFields = new List<string>{ "Side", "Height" },
                            Compute = input =>
                            {
                                float a = input["Side"];
                                float h = input["Height"];
                                return (1f / 3f) * a * a * h;
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

            if (!result.ContainsKey("Side"))
                throw new Exception("Thiếu độ dài cạnh đáy.");

            float a = result["Side"];
            float h = result.ContainsKey("Height") ? result["Height"] : Mathf.Sqrt(a * a - (a * a / 2f));

            Transform lookingPoint = CameraController.Instance.target;
            
            
            
            Vector3 A = lookingPoint.position - new Vector3(a/2, 0, a/2);
            Vector3 B = A + new Vector3(a, 0, 0);
            Vector3 C = A + new Vector3(a, 0, a);
            Vector3 D = A + new Vector3(0, 0, a);
            Vector3 Apex = A + new Vector3(a / 2f, h, a / 2f);

            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();
            string idS = Guid.NewGuid().ToString();

            var data = new List<ShapeData>
            {
                new() { Id = idA, Type = "Point", Position = A, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idB, Type = "Point", Position = B, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idC, Type = "Point", Position = C, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idD, Type = "Point", Position = D, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idS, Type = "Point", Position = Apex, Rotation = Quaternion.identity, Scale = Vector3.one },

                // Base
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idB }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idC }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idD }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD, idA }},

                // Sides
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idS }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idS }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idS }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD, idS }}
            };
            
            

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(data));
            
            MeshGenerator.MeshCompute(new []{A, B, C, D, Apex},
                new []{
                    0, 1, 2, 
                    0, 2, 3,
                    0, 1, 4,
                    0, 3, 4,
                    1, 2, 4,
                    2, 3, 4
                }, new []
                {
                    ShapeStorage.GetById(idA).gameObject.transform, 
                    ShapeStorage.GetById(idB).gameObject.transform, 
                    ShapeStorage.GetById(idC).gameObject.transform, 
                    ShapeStorage.GetById(idD).gameObject.transform, 
                    ShapeStorage.GetById(idS).gameObject.transform, 
                }
            );
            return data;
        }
    }
}
