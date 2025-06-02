using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class SquarePyramid : Shape, ShapeMesh
    {
        public Point A, B, C, D, S; // 4 base, 1 apex
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        protected override void Awake()
        {
            base.Awake();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = MeshMat;

            // No guarantee points are assigned at Awake, so subscribe in Initialize or WaitAndReconnect.
        }

        public void Initialize(Point a, Point b, Point c, Point d, Point apex)
        {
            A = a; B = b; C = c; D = d; S = apex;
            AddPivot(A); AddPivot(B); AddPivot(C); AddPivot(D); AddPivot(S);

            A.OnPositionChanged += _ => UpdateMesh();
            B.OnPositionChanged += _ => UpdateMesh();
            C.OnPositionChanged += _ => UpdateMesh();
            D.OnPositionChanged += _ => UpdateMesh();
            S.OnPositionChanged += _ => UpdateMesh();

            UpdateMesh();
        }

        public override void UpdateHitbox() { /* Add collider if needed */ }

        public override void MoveTo(Vector3 newPosition, bool silent = false, bool queue = true)
        {
            // Move all base and apex points accordingly
        }

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "SquarePyramid";
            data.ConnectedPoints = new List<string> { A.ShapeId, B.ShapeId, C.ShapeId, D.ShapeId, S.ShapeId };
            return data;
        }

        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);
            if (data.ConnectedPoints.Count == 5)
                StartCoroutine(WaitAndReconnect(data.ConnectedPoints));
        }

        private IEnumerator WaitAndReconnect(List<string> pointIds)
        {
            // Wait for all points to be registered in ShapeStorage
            while (pointIds.Exists(id => ShapeStorage.GetById(id) == null))
                yield return null;

            A = ShapeStorage.GetById(pointIds[0]) as Point;
            B = ShapeStorage.GetById(pointIds[1]) as Point;
            C = ShapeStorage.GetById(pointIds[2]) as Point;
            D = ShapeStorage.GetById(pointIds[3]) as Point;
            S = ShapeStorage.GetById(pointIds[4]) as Point;

            AddPivot(A); AddPivot(B); AddPivot(C); AddPivot(D); AddPivot(S);

            A.OnPositionChanged += _ => UpdateMesh();
            B.OnPositionChanged += _ => UpdateMesh();
            C.OnPositionChanged += _ => UpdateMesh();
            D.OnPositionChanged += _ => UpdateMesh();
            S.OnPositionChanged += _ => UpdateMesh();

            UpdateMesh();
        }

        private void UpdateMesh()
        {
            if (meshFilter == null || A == null || B == null || C == null || D == null || S == null) return;
            var mesh = new Mesh();
            var verts = new[]
            {
                A.transform.position - transform.position,
                B.transform.position - transform.position,
                C.transform.position - transform.position,
                D.transform.position - transform.position,
                S.transform.position - transform.position
            };
            mesh.vertices = verts;
            mesh.triangles = new[]
            {
                0, 1, 2, // base
                0, 2, 3,
                0, 1, 4, // sides
                1, 2, 4,
                2, 3, 4,
                3, 0, 4
            };
            mesh.RecalculateNormals();
            meshFilter.sharedMesh = mesh;
        }
    }
}
