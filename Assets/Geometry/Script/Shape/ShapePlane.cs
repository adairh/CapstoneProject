using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class PlaneShape : Shape
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        public override void InitializeNew(string type, Vector3 position)
        {
            base.InitializeNew(type, position);

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = MaterialLibrary.Get(MaterialType.Default);
        }

        public void SetPoints(List<Point> points)
        {
            pivotPoints.Clear();
            foreach (var p in points)
                AddPivot(p);

            meshFilter.mesh = MeshGenerator.CreatePlane(points);
        }

        public event System.Action<Mesh> OnMeshUpdated;

        public override void CompleteDraw()
        {
            base.CompleteDraw();
            var mesh = MeshGenerator.CreatePlane(pivotPoints);
            GetComponent<MeshFilter>().mesh = mesh;
            OnMeshUpdated?.Invoke(mesh);
        }

        public override void UpdateHitbox()
        {
            // Optional: Add bounds or collider for the plane if needed
        }

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "Plane";
            return data;
        }

        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);
            meshFilter.mesh = MeshGenerator.CreatePlane(pivotPoints);
        }
    }
}