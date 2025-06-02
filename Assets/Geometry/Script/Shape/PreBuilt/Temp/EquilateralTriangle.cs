using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class EquilateralTriangle : Shape, ShapeMesh
    {
        public Point A, B, C;
        public Segment AB, BC, CA;

        // Mesh references
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        // Track events
        private bool eventsRegistered = false;

        protected override void Awake()
        {
            base.Awake();

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = MeshMat;
        }

        // For direct creation
        public void Initialize(Point a, Point b, Point c, Segment ab, Segment bc, Segment ca)
        {
            A = a; B = b; C = c;
            AB = ab; BC = bc; CA = ca;

            AddPivot(A); AddPivot(B); AddPivot(C);
            RegisterPointEvents();
            UpdateMesh();
        }

        // For deserialization from ShapeData
        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);
            if (data.ConnectedPoints.Count == 3)
                StartCoroutine(WaitAndReconnect(data.ConnectedPoints));
        }

        private IEnumerator WaitAndReconnect(List<string> pointIds)
        {
            // Wait for points to be spawned and registered
            while (ShapeStorage.GetById(pointIds[0]) == null ||
                   ShapeStorage.GetById(pointIds[1]) == null ||
                   ShapeStorage.GetById(pointIds[2]) == null)
                yield return null;

            A = ShapeStorage.GetById(pointIds[0]) as Point;
            B = ShapeStorage.GetById(pointIds[1]) as Point;
            C = ShapeStorage.GetById(pointIds[2]) as Point;

            AddPivot(A); AddPivot(B); AddPivot(C);
            RegisterPointEvents();
            UpdateMesh();
        }

        // Event management
        private void RegisterPointEvents()
        {
            if (eventsRegistered) return;
            if (A) A.OnPositionChanged += OnAnyPointMoved;
            if (B) B.OnPositionChanged += OnAnyPointMoved;
            if (C) C.OnPositionChanged += OnAnyPointMoved;
            eventsRegistered = true;
        }
        private void UnregisterPointEvents()
        {
            if (!eventsRegistered) return;
            if (A) A.OnPositionChanged -= OnAnyPointMoved;
            if (B) B.OnPositionChanged -= OnAnyPointMoved;
            if (C) C.OnPositionChanged -= OnAnyPointMoved;
            eventsRegistered = false;
        }
        private void OnAnyPointMoved(Point p) => UpdateMesh();

        // Unity callback for cleanup
        protected override void OnDestroy()
        {
            UnregisterPointEvents();
            base.OnDestroy();
        }

        private void UpdateMesh()
        {
            if (meshFilter == null || A == null || B == null || C == null) return;

            // World-space vertices for a stable mesh
            Vector3[] vertices = new[]
            {
                A.transform.position,
                B.transform.position,
                C.transform.position
            };

            // Center the mesh on this object's transform
            Vector3 center = (vertices[0] + vertices[1] + vertices[2]) / 3f;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] -= center;
            transform.position = center;

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.RecalculateNormals();
            meshFilter.sharedMesh = mesh;
        }

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "EquilateralTriangle";
            data.ConnectedPoints = new List<string> { A.ShapeId, B.ShapeId, C.ShapeId };
            return data;
        }

        public override void UpdateHitbox()
        {
            // You can optionally add/update a collider here
        }

        public override void MoveTo(Vector3 newPosition, bool silent = false, bool queue = true)
        {
            // Move all points to maintain triangle shape
            if (A && B && C)
            {
                Vector3 centroid = (A.transform.position + B.transform.position + C.transform.position) / 3f;
                Vector3 delta = newPosition - centroid;
                A.MoveTo(A.transform.position + delta, silent, queue);
                B.MoveTo(B.transform.position + delta, silent, queue);
                C.MoveTo(C.transform.position + delta, silent, queue);
            }
        }

        // Optional: area, side, etc. properties
        public float Side => A && B ? Vector3.Distance(A.transform.position, B.transform.position) : 0f;
        public float Area => Mathf.Sqrt(3f) / 4f * Side * Side;
    }
}
