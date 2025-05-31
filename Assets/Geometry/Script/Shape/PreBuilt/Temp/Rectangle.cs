
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Manipulator
{
    public class Rectangle : Shape
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        private GameObject visual;
        public List<Point> Points { get; } = new();

        protected override void Awake()
        {
            base.Awake();

            visual = new GameObject("RectangleMesh");
            visual.transform.SetParent(transform, false);

            meshFilter = visual.AddComponent<MeshFilter>();
            meshRenderer = visual.AddComponent<MeshRenderer>();
            meshCollider = visual.AddComponent<MeshCollider>();

            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var p in Points)
                if (p != null)
                    p.OnPositionChanged -= OnPivotMoved;
        }

        public void SetPoints(List<Point> pts)
        {
            foreach (var p in Points)
                if (p != null)
                    p.OnPositionChanged -= OnPivotMoved;

            Points.Clear();
            foreach (var p in pts)
            {
                Points.Add(p);
                AddPivot(p);
                p.OnPositionChanged += OnPivotMoved;
            }
            GenerateMesh();
        }

        private void GenerateMesh()
        {
            if (Points.Count < 3) return;

            var mesh = MeshGenerator.GenerateMesh(Points.Select(p => p.transform.position).ToList());
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = Points.Count >= 4;

            // Only assign the mesh material ONCE
            if (meshRenderer.sharedMaterial != MeshMat)
                meshRenderer.sharedMaterial = MeshMat;

            // Optional: Highlight
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", Color.cyan);
            meshRenderer.SetPropertyBlock(block);
        }

        public void SetMeshHighlightColor(Color color)
        {
            var block = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            meshRenderer.SetPropertyBlock(block);
        }

        private void OnPivotMoved(Point pt)
        {
            GenerateMesh();
        }

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "Rectangle";
            data.ConnectedPoints = Points.Select(p => p.ShapeId).ToList();
            return data;
        }

        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);
            SetPoints(data.ConnectedPoints
                .Select(id => ShapeStorage.GetById(id) as Point)
                .Where(p => p != null).ToList());
        }

        public override IEnumerable<Shape> GetDependentShapesForDelete()
        {
            yield return this;
            foreach (var p in Points)
                if (p != null && p.IsOnlyConnectedTo(this))
                    yield return p;
        }
    }
}
