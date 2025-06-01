
using System;
using System.Collections.Generic;
using UnityEngine; 

namespace Manipulator
{
    public class SquarePyramidSpawner : IShapeSpawner
    {
        private string idA, idB, idC, idD, idApex;
        private string idAB, idBC, idCD, idDA, idAApex, idBApex, idCApex, idDApex;

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
                            InputFields = new List<string> { "Volume", "Height" },
                            Compute = input => Mathf.Sqrt((3f * input["Volume"]) / input["Height"])
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
                            InputFields = new List<string> { "Volume", "BaseSide" },
                            Compute = input => (3f * input["Volume"]) / Mathf.Pow(input["BaseSide"], 2)
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
                            InputFields = new List<string> { "BaseSide", "Height" },
                            Compute = input => (1f / 3f) * Mathf.Pow(input["BaseSide"], 2) * input["Height"]
                        }
                    }
                }
            };
        }

        public List<ShapeData> ComputeShape(Dictionary<string, float> inputs)
        {
            var fieldSolver = new FieldSolver(GetFieldDefinitions());
            var result = fieldSolver.Solve(inputs);

            if (!result.ContainsKey("BaseSide") || !result.ContainsKey("Height"))
                throw new Exception("Không thể suy luận đủ dữ kiện để dựng hình chóp.");

            float a = result["BaseSide"];
            float h = result["Height"]; 
            Transform lookingPoint = CameraController.Instance.target;


            var position = lookingPoint.position;
            Vector3 A = new Vector3(-a / 2, 0, -a / 2) + position;
            Vector3 B = new Vector3(a / 2, 0, -a / 2) + position;
            Vector3 C = new Vector3(a / 2, 0, a / 2) + position;
            Vector3 D = new Vector3(-a / 2, 0, a / 2) + position;
            Vector3 S = new Vector3(0, h, 0) + position;

            idA = Guid.NewGuid().ToString();
            idB = Guid.NewGuid().ToString();
            idC = Guid.NewGuid().ToString();
            idD = Guid.NewGuid().ToString();
            idApex = Guid.NewGuid().ToString();
            idAB = Guid.NewGuid().ToString();
            idBC = Guid.NewGuid().ToString();
            idCD = Guid.NewGuid().ToString();
            idDA = Guid.NewGuid().ToString();
            idAApex = Guid.NewGuid().ToString();
            idBApex = Guid.NewGuid().ToString();
            idCApex = Guid.NewGuid().ToString();
            idDApex = Guid.NewGuid().ToString();

            var shapes = new List<ShapeData>
            {
                new() { Id = idA, Type = "Point", Position = A, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idB, Type = "Point", Position = B, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idC, Type = "Point", Position = C, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idD, Type = "Point", Position = D, Rotation = Quaternion.identity, Scale = Vector3.one },
                new() { Id = idApex, Type = "Point", Position = S, Rotation = Quaternion.identity, Scale = Vector3.one },

                new() { Id = idAB, Type = "Segment", ConnectedPoints = new List<string> { idA, idB } },
                new() { Id = idBC, Type = "Segment", ConnectedPoints = new List<string> { idB, idC } },
                new() { Id = idCD, Type = "Segment", ConnectedPoints = new List<string> { idC, idD } },
                new() { Id = idDA, Type = "Segment", ConnectedPoints = new List<string> { idD, idA } },

                new() { Id = idAApex, Type = "Segment", ConnectedPoints = new List<string> { idA, idApex } },
                new() { Id = idBApex, Type = "Segment", ConnectedPoints = new List<string> { idB, idApex } },
                new() { Id = idCApex, Type = "Segment", ConnectedPoints = new List<string> { idC, idApex } },
                new() { Id = idDApex, Type = "Segment", ConnectedPoints = new List<string> { idD, idApex } }
            };
            
            
            
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(shapes));
            
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
                    ShapeStorage.GetById(idApex).gameObject.transform, 
                });
            return shapes;
        }
    }
}
