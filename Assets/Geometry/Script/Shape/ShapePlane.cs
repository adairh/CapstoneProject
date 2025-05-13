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
            meshRenderer.material = DefaultMat;
        }

        public override void UpdateHitbox()
        {
            if (pivotPoints.Count < 3) return;

            Vector3 p0 = pivotPoints[0].transform.position;
            Vector3 p1 = pivotPoints[1].transform.position;
            Vector3 p2 = pivotPoints[2].transform.position;

            Vector3 center = (p0 + p1 + p2) / 3f;
            Vector3 dir1 = (p1 - p0);
            Vector3 dir2 = (p2 - p0);

            float extent1 = dir1.magnitude;
            float extent2 = dir2.magnitude;

            Vector3 normal = Vector3.Cross(dir1, dir2).normalized;
            Vector3 upHint = Vector3.Cross(dir1, normal); // giúp cố định "trục lên" của collider

            transform.position = center;
            transform.rotation = Quaternion.LookRotation(normal, upHint); // ✅ chuẩn hóa hướng xoay

            var box = gameObject.GetComponent<BoxCollider>();
            if (!box) box = gameObject.AddComponent<BoxCollider>();

            box.center = Vector3.zero;
            box.size = new Vector3(extent1 * 2, extent2 * 2, 0.1f); // 0.1f là độ dày pháp tuyến
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
            UpdateHitbox();
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