using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class GenericPyramid : Shape, ShapeMesh
    {
        public Point A, B, C, D, S; // Base: A-B-C-D; Apex: S
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private bool eventsRegistered = false;

        protected override void Awake()
        {
            base.Awake();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = MeshMat;
        }

        public void Initialize(Point a, Point b, Point c, Point d, Point s)
        {
            A = a; B = b; C = c; D = d; S = s;
            AddPivot(A); AddPivot(B); AddPivot(C); AddPivot(D); AddPivot(S);
            RegisterPointEvents();
            UpdateMesh();
        }

        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);
            if (data.ConnectedPoints.Count == 5)
                StartCoroutine(WaitAndReconnect(data.ConnectedPoints));
        }

        private IEnumerator WaitAndReconnect(List<string> pointIds)
        {
            while (
                ShapeStorage.GetById(pointIds[0]) == null ||
                ShapeStorage.GetById(pointIds[1]) == null ||
                ShapeStorage.GetById(pointIds[2]) == null ||
                ShapeStorage.GetById(pointIds[3]) == null ||
                ShapeStorage.GetById(pointIds[4]) == null
            )
                yield return null;

            A = ShapeStorage.GetById(pointIds[0]) as Point;
            B = ShapeStorage.GetById(pointIds[1]) as Point;
            C = ShapeStorage.GetById(pointIds[2]) as Point;
            D = ShapeStorage.GetById(pointIds[3]) as Point;
            S = ShapeStorage.GetById(pointIds[4]) as Point;

            AddPivot(A); AddPivot(B); AddPivot(C); AddPivot(D); AddPivot(S);
            RegisterPointEvents();
            UpdateMesh();
        }

        private void RegisterPointEvents()
        {
            if (eventsRegistered) return;
            if (A) A.OnPositionChanged += OnAnyPointMoved;
            if (B) B.OnPositionChanged += OnAnyPointMoved;
            if (C) C.OnPositionChanged += OnAnyPointMoved;
            if (D) D.OnPositionChanged += OnAnyPointMoved;
            if (S) S.OnPositionChanged += OnAnyPointMoved;
            eventsRegistered = true;
        }
        private void UnregisterPointEvents()
        {
            if (!eventsRegistered) return;
            if (A) A.OnPositionChanged -= OnAnyPointMoved;
            if (B) B.OnPositionChanged -= OnAnyPointMoved;
            if (C) C.OnPositionChanged -= OnAnyPointMoved;
            if (D) D.OnPositionChanged -= OnAnyPointMoved;
            if (S) S.OnPositionChanged -= OnAnyPointMoved;
            eventsRegistered = false;
        }
        private void OnAnyPointMoved(Point p) => UpdateMesh();

        protected override void OnDestroy()
        {
            UnregisterPointEvents();
            base.OnDestroy();
        }

        private void UpdateMesh()
        {
            if (meshFilter == null || A == null || B == null || C == null || D == null || S == null) return;
            var mesh = new Mesh();
            var center = (A.transform.position + B.transform.position + C.transform.position + D.transform.position + S.transform.position) / 5f;
            var verts = new[]
            {
                A.transform.position - center,
                B.transform.position - center,
                C.transform.position - center,
                D.transform.position - center,
                S.transform.position - center
            };
            mesh.vertices = verts;
            mesh.triangles = new[]
            {
                0, 1, 2,   // base
                0, 2, 3,
                0, 1, 4,   // sides
                1, 2, 4,
                2, 3, 4,
                3, 0, 4
            };
            mesh.RecalculateNormals();
            meshFilter.sharedMesh = mesh;
            transform.position = center;
        }

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "GenericPyramid";
            data.ConnectedPoints = new List<string> { A.ShapeId, B.ShapeId, C.ShapeId, D.ShapeId, S.ShapeId };
            return data;
        }

        public override void UpdateHitbox() { }
        public override void MoveTo(Vector3 newPosition, bool silent = false, bool queue = true)
        {
            if (A && B && C && D && S)
            {
                Vector3 centroid = (A.transform.position + B.transform.position + C.transform.position + D.transform.position + S.transform.position) / 5f;
                Vector3 delta = newPosition - centroid;
                A.MoveTo(A.transform.position + delta, silent, queue);
                B.MoveTo(B.transform.position + delta, silent, queue);
                C.MoveTo(C.transform.position + delta, silent, queue);
                D.MoveTo(D.transform.position + delta, silent, queue);
                S.MoveTo(S.transform.position + delta, silent, queue);
            }
        }
    }
}
