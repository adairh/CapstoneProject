using System.Collections;
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

        private List<Point> pendingPoints;

        public void Initialize(List<Point> points)
        {
            if (points == null || points.Count < 3) return;

            pendingPoints = points;
            StartCoroutine(SafeInitMesh());
        }

        private IEnumerator SafeInitMesh()
        {
            while (!AreAllPointsReady())
                yield return null;

            var positions = new List<Vector3>();
            foreach (var p in pendingPoints)
                positions.Add(p.transform.position);

            meshFilter = gameObject.GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();

            Material fallback = MaterialLibrary.Get(MaterialType.Default);
            if (fallback == null)
            {
                Debug.LogWarning("No mesh material assigned and MaterialLibrary.Default is missing. Using built-in Standard.");
                fallback = new Material(Shader.Find("Standard"));
            }

            meshRenderer.material = meshMaterial ?? fallback;
            meshFilter.mesh = MeshGenerator.GenerateMesh(positions);
        }

        private bool AreAllPointsReady()
        {
            foreach (var p in pendingPoints)
            {
                if (p == null || p.transform == null || !p.gameObject.activeInHierarchy)
                    return false;
            }
            return true;
        }

        public void Clear()
        {
            if (meshFilter != null) Destroy(meshFilter);
            if (meshRenderer != null) Destroy(meshRenderer);
        }
    }
}
