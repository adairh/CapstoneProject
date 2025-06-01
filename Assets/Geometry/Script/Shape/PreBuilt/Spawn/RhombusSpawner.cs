
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
                new FieldDefinition { Name = "Side", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Diagonal1", Type = FieldType.Length, IsRequired = false },
                new FieldDefinition { Name = "Diagonal2", Type = FieldType.Length, IsRequired = false },
                new FieldDefinition
                {
                    Name = "Area",
                    Type = FieldType.Area,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule
                        {
                            InputFields = new List<string>{ "Diagonal1", "Diagonal2" },
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

            if (!result.ContainsKey("Side"))
                throw new Exception("Thiếu độ dài cạnh.");

            float a = result["Side"];
            float d1 = result.ContainsKey("Diagonal1") ? result["Diagonal1"] : a * Mathf.Sqrt(2);
            float d2 = result.ContainsKey("Diagonal2") ? result["Diagonal2"] : a * Mathf.Sqrt(2);

            Transform lookingPoint = CameraController.Instance.target;


            Vector3 A = lookingPoint.position + new Vector3(-d1 / 2, 0, 0);
            Vector3 C = lookingPoint.position + new Vector3(d1 / 2, 0, 0);
            Vector3 B = lookingPoint.position + new Vector3(0, 0, d2 / 2);
            Vector3 D = lookingPoint.position + new Vector3(0, 0, -d2 / 2);

            string idA = Guid.NewGuid().ToString();
            string idB = Guid.NewGuid().ToString();
            string idC = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();

            var data = new List<ShapeData>
            {
                new() {Id = idA, Type = "Point", Position = A, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() {Id = idB, Type = "Point", Position = B, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() {Id = idC, Type = "Point", Position = C, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() {Id = idD, Type = "Point", Position = D, Rotation = Quaternion.identity, Scale = Vector3.one },

                new() {Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idB } },
                new() {Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idC } },
                new() {Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idD } },
                new() {Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD, idA } }
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(data));
            MeshGenerator.MeshCompute(new []{A, B, C, D}, new []{0, 1, 2, 0, 2, 3 }, new []
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
