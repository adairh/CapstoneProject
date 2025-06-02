using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class Rectangle : Shape, ShapeMesh
    {
        public Point A, B, C, D;
        public Segment AB, BC, CD, DA;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private bool eventsRegistered = false;

        protected override void Awake()
        {
            base.Awake();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = MeshMat; // Use MeshMat for polygonal shapes
        }

        public void Initialize(Point a, Point b, Point c, Point d, Segment ab, Segment bc, Segment cd, Segment da)
        {
            A = a; B = b; C = c; D = d;
            AB = ab; BC = bc; CD = cd; DA = da;

            AddPivot(A); AddPivot(B); AddPivot(C); AddPivot(D);
            RegisterPointEvents();
            UpdateMesh();
        }

        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);
            if (data.ConnectedPoints.Count == 4)
                StartCoroutine(WaitAndReconnect(data.ConnectedPoints));
        }

        private IEnumerator WaitAndReconnect(List<string> pointIds)
        {
            while (ShapeStorage.GetById(pointIds[0]) == null ||
                   ShapeStorage.GetById(pointIds[1]) == null ||
                   ShapeStorage.GetById(pointIds[2]) == null ||
                   ShapeStorage.GetById(pointIds[3]) == null)
                yield return null;

            A = ShapeStorage.GetById(pointIds[0]) as Point;
            B = ShapeStorage.GetById(pointIds[1]) as Point;
            C = ShapeStorage.GetById(pointIds[2]) as Point;
            D = ShapeStorage.GetById(pointIds[3]) as Point;

            AddPivot(A); AddPivot(B); AddPivot(C); AddPivot(D);
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
            eventsRegistered = true;
        }
        private void UnregisterPointEvents()
        {
            if (!eventsRegistered) return;
            if (A) A.OnPositionChanged -= OnAnyPointMoved;
            if (B) B.OnPositionChanged -= OnAnyPointMoved;
            if (C) C.OnPositionChanged -= OnAnyPointMoved;
            if (D) D.OnPositionChanged -= OnAnyPointMoved;
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
            if (meshFilter == null || A == null || B == null || C == null || D == null) return;

            Vector3[] vertices = new[]
            {
                A.transform.position,
                B.transform.position,
                C.transform.position,
                D.transform.position
            };

            Vector3 center = (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4f;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] -= center;
            transform.position = center;

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 }; // Two triangles
            mesh.RecalculateNormals();
            meshFilter.sharedMesh = mesh;
        }

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "Rectangle";
            data.ConnectedPoints = new List<string> { A.ShapeId, B.ShapeId, C.ShapeId, D.ShapeId };
            return data;
        }

        public override void UpdateHitbox() { }
        public override void MoveTo(Vector3 newPosition, bool silent = false, bool queue = true)
        {
            if (A && B && C && D)
            {
                Vector3 centroid = (A.transform.position + B.transform.position + C.transform.position + D.transform.position) / 4f;
                Vector3 delta = newPosition - centroid;
                A.MoveTo(A.transform.position + delta, silent, queue);
                B.MoveTo(B.transform.position + delta, silent, queue);
                C.MoveTo(C.transform.position + delta, silent, queue);
                D.MoveTo(D.transform.position + delta, silent, queue);
            }
        }

        // Optional measurements
        public float Width => (A && B) ? Vector3.Distance(A.transform.position, B.transform.position) : 0f;
        public float Height => (B && C) ? Vector3.Distance(B.transform.position, C.transform.position) : 0f;
        public float Area => Width * Height;
        public float Diagonal => (A && C) ? Vector3.Distance(A.transform.position, C.transform.position) : 0f;
    }
}
