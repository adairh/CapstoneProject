// ShapeMeshDisplay.cs
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ShapeMeshDisplay : MonoBehaviour
    {
        [SerializeField] private Material meshMaterial;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        public void Initialize(List<Point> points)
        {
            if (points == null || points.Count < 3) return;

            List<Vector3> positions = new();
            foreach (var p in points)
                positions.Add(p.transform.position);

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshRenderer.material = meshMaterial != null
                ? meshMaterial
                : MaterialLibrary.Get(MaterialType.Default);

            meshFilter.mesh = MeshGenerator.GenerateMesh(positions);
        }

        public void Clear()
        {
            if (meshFilter != null) Destroy(meshFilter);
            if (meshRenderer != null) Destroy(meshRenderer);
        }
    }
}
