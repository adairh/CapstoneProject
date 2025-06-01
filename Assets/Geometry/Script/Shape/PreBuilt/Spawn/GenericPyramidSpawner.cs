
using System;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Manipulator
{
    public class GenericPyramidSpawner : IShapeSpawner
    {
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition { Name = "BaseLength", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "BaseWidth", Type = FieldType.Length, IsRequired = true },
                new FieldDefinition { Name = "Height", Type = FieldType.Length, IsRequired = true },

                new FieldDefinition
                {
                    Name = "BaseArea",
                    Type = FieldType.Area,
                    IsRequired = false,
                    ComputeRules = new List<ComputeRule>
                    {
                        new ComputeRule {
                            InputFields = new List<string>{ "BaseLength", "BaseWidth" },
                            Compute = input => input["BaseLength"] * input["BaseWidth"]
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
                            InputFields = new List<string>{ "BaseLength", "BaseWidth", "Height" },
                            Compute = input => (input["BaseLength"] * input["BaseWidth"] * input["Height"]) / 3f
                        }
                    }
                }
            };
        }

        public List<ShapeData> ComputeShape(Dictionary<string, float> inputs)
        {
            var solver = new FieldSolver(GetFieldDefinitions());
            var result = solver.Solve(inputs);

            if (!(result.ContainsKey("BaseLength") && result.ContainsKey("BaseWidth") && result.ContainsKey("Height")))
                throw new Exception("Thiếu dữ kiện cạnh đáy và chiều cao.");

            float l = result["BaseLength"];
            float w = result["BaseWidth"];
            float h = result["Height"];
             
            
            Transform lookingPoint = CameraController.Instance.target;
            
            Vector3 A = (lookingPoint.position + new Vector3(0, 0.5f, 0)) - new Vector3(l/2, 0, w/2);
            Vector3 B = A + new Vector3(l, 0, 0);
            Vector3 C = B + new Vector3(0, 0, w);
            Vector3 D = A + new Vector3(0, 0, w);
            Vector3 S = A + new Vector3(l / 2f, h, w / 2f); // Apex

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
                new() { Id = idS, Type = "Point", Position = S, Rotation = Quaternion.identity, Scale = Vector3.one },

                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idA, idB }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idB, idC }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idC, idD }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idD, idA }},

                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idS, idA }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idS, idB }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idS, idC }},
                new() { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new List<string>{ idS, idD }}
            };
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(data));
            MeshGenerator.MeshCompute(new []{A, B, C, D, S},
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
                });
            return data;
        }
    }
}
