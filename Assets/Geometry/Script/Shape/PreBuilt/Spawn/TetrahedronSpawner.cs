
using System;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Manipulator
{
    public class TetrahedronSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "BaseSide", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "BaseHeight", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "ApexHeight", Type = FieldType.Length, IsRequired = true },

                new FieldDefinition
                {
                    Name = "Volume",
                    Type = FieldType.Volume,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "BaseSide", "BaseHeight", "ApexHeight" },
                            Compute = input =>
                            {
                                float baseArea = 0.5f * input["BaseSide"] * input["BaseHeight"];
                                return (1f / 3f) * baseArea * input["ApexHeight"];
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

            if (!(result.ContainsKey("BaseSide") && result.ContainsKey("BaseHeight") && result.ContainsKey("ApexHeight")))
                throw new Exception("Thiếu dữ kiện cơ sở để dựng hình.");

            float baseSide = result["BaseSide"];
            float baseHeight = result["BaseHeight"];
            float apexHeight = result["ApexHeight"];

            Transform lookingPoint = CameraController.Instance.target;

            Vector3 A = lookingPoint.position - new Vector3(baseSide / 2, 0, baseHeight / 2);
            Vector3 B = A + new Vector3(baseSide, 0, 0);
            Vector3 C = A + new Vector3(baseSide / 2f, 0, baseHeight);
            Vector3 D = A + new Vector3(baseSide / 2f, apexHeight, baseHeight / 3f);

            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();

            var data = new List<ShapeData>
            {
                new() { Id = idA, Type = "Point", Position = A, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idB, Type = "Point", Position = B, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idC, Type = "Point", Position = C, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idD, Type = "Point", Position = D, Rotation = Quaternion.identity, Scale = Vector3.one },

                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idB }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idC }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idA }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idD }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idD }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idD }}
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(data));
            MeshGenerator.MeshCompute(new []{A, B, C, D}, 
                new []
                {
                    0, 1, 2, 
                    0, 2, 3,
                    0, 1, 3,
                    1, 2, 3,
                }, new []
                {
                    ShapeStorage.GetById(idA).gameObject.transform, 
                    ShapeStorage.GetById(idB).gameObject.transform, 
                    ShapeStorage.GetById(idC).gameObject.transform, 
                    ShapeStorage.GetById(idD).gameObject.transform,  
                });
            return data;
        }
    }
}
