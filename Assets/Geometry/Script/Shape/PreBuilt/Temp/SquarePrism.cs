using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class SquarePrism : Shape
    {
        // Vertices: A, B, C, D (bottom); A2, B2, C2, D2 (top)
        public Point A, B, C, D, A2, B2, C2, D2;
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

        public void Initialize(Point a, Point b, Point c, Point d, Point a2, Point b2, Point c2, Point d2)
        {
            A = a; B = b; C = c; D = d; A2 = a2; B2 = b2; C2 = c2; D2 = d2;
            AddPivot(A); AddPivot(B); AddPivot(C); AddPivot(D);
            AddPivot(A2); AddPivot(B2); AddPivot(C2); AddPivot(D2);
            RegisterPointEvents();
            UpdateMesh();
        }

        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);
            if (data.ConnectedPoints.Count == 8)
                StartCoroutine(WaitAndReconnect(data.ConnectedPoints));
        }

        private IEnumerator WaitAndReconnect(List<string> pointIds)
        {
            while (
                ShapeStorage.GetById(pointIds[0]) == null ||
                ShapeStorage.GetById(pointIds[1]) == null ||
                ShapeStorage.GetById(pointIds[2]) == null ||
                ShapeStorage.GetById(pointIds[3]) == null ||
                ShapeStorage.GetById(pointIds[4]) == null ||
                ShapeStorage.GetById(pointIds[5]) == null ||
                ShapeStorage.GetById(pointIds[6]) == null ||
                ShapeStorage.GetById(pointIds[7]) == null
            )
                yield return null;

            A = ShapeStorage.GetById(pointIds[0]) as Point;
            B = ShapeStorage.GetById(pointIds[1]) as Point;
            C = ShapeStorage.GetById(pointIds[2]) as Point;
            D = ShapeStorage.GetById(pointIds[3]) as Point;
            A2 = ShapeStorage.GetById(pointIds[4]) as Point;
            B2 = ShapeStorage.GetById(pointIds[5]) as Point;
            C2 = ShapeStorage.GetById(pointIds[6]) as Point;
            D2 = ShapeStorage.GetById(pointIds[7]) as Point;

            AddPivot(A); AddPivot(B); AddPivot(C); AddPivot(D);
            AddPivot(A2); AddPivot(B2); AddPivot(C2); AddPivot(D2);
            RegisterPointEvents();
            UpdateMesh();
        }

        private void RegisterPointEvents()
        {
            if (eventsRegistered) return;
            if (A)  A.OnPositionChanged  += OnAnyPointMoved;
            if (B)  B.OnPositionChanged  += OnAnyPointMoved;
            if (C)  C.OnPositionChanged  += OnAnyPointMoved;
            if (D)  D.OnPositionChanged  += OnAnyPointMoved;
            if (A2) A2.OnPositionChanged += OnAnyPointMoved;
            if (B2) B2.OnPositionChanged += OnAnyPointMoved;
            if (C2) C2.OnPositionChanged += OnAnyPointMoved;
            if (D2) D2.OnPositionChanged += OnAnyPointMoved;
            eventsRegistered = true;
        }
        private void UnregisterPointEvents()
        {
            if (!eventsRegistered) return;
            if (A)  A.OnPositionChanged  -= OnAnyPointMoved;
            if (B)  B.OnPositionChanged  -= OnAnyPointMoved;
            if (C)  C.OnPositionChanged  -= OnAnyPointMoved;
            if (D)  D.OnPositionChanged  -= OnAnyPointMoved;
            if (A2) A2.OnPositionChanged -= OnAnyPointMoved;
            if (B2) B2.OnPositionChanged -= OnAnyPointMoved;
            if (C2) C2.OnPositionChanged -= OnAnyPointMoved;
            if (D2) D2.OnPositionChanged -= OnAnyPointMoved;
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
            if (meshFilter == null || A == null || B == null || C == null || D == null
                || A2 == null || B2 == null || C2 == null || D2 == null) return;

            // Center for local mesh vertices
            Vector3 center = (A.transform.position + B.transform.position + C.transform.position + D.transform.position
                              + A2.transform.position + B2.transform.position + C2.transform.position + D2.transform.position) / 8f;

            var verts = new[]
            {
                A.transform.position - center,  // 0
                B.transform.position - center,  // 1
                C.transform.position - center,  // 2
                D.transform.position - center,  // 3
                A2.transform.position - center, // 4
                B2.transform.position - center, // 5
                C2.transform.position - center, // 6
                D2.transform.position - center  // 7
            };

            // 12 triangles (2 per face, 6 faces)
            var tris = new[]
            {
                // Bottom face
                0, 1, 2,
                0, 2, 3,
                // Top face
                4, 6, 5,
                4, 7, 6,
                // Front face
                0, 4, 5,
                0, 5, 1,
                // Right face
                1, 5, 6,
                1, 6, 2,
                // Back face
                2, 6, 7,
                2, 7, 3,
                // Left face
                3, 7, 4,
                3, 4, 0
            };

            var mesh = new Mesh();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            meshFilter.sharedMesh = mesh;
            transform.position = center;
        }

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "SquarePrism";
            data.ConnectedPoints = new List<string>
            {
                A.ShapeId, B.ShapeId, C.ShapeId, D.ShapeId, A2.ShapeId, B2.ShapeId, C2.ShapeId, D2.ShapeId
            };
            return data;
        }

        public override void UpdateHitbox() { }

        public override void MoveTo(Vector3 newPosition, bool silent = false, bool queue = true)
        {
            if (A && B && C && D && A2 && B2 && C2 && D2)
            {
                Vector3 centroid = (A.transform.position + B.transform.position + C.transform.position + D.transform.position
                                   + A2.transform.position + B2.transform.position + C2.transform.position + D2.transform.position) / 8f;
                Vector3 delta = newPosition - centroid;
                A.MoveTo(A.transform.position + delta, silent, queue);
                B.MoveTo(B.transform.position + delta, silent, queue);
                C.MoveTo(C.transform.position + delta, silent, queue);
                D.MoveTo(D.transform.position + delta, silent, queue);
                A2.MoveTo(A2.transform.position + delta, silent, queue);
                B2.MoveTo(B2.transform.position + delta, silent, queue);
                C2.MoveTo(C2.transform.position + delta, silent, queue);
                D2.MoveTo(D2.transform.position + delta, silent, queue);
            }
        }
    }
}
