using UnityEngine;
using UnityEngine.Rendering;

namespace Manipulator
{
    public class GridAndAxisSystem : MonoBehaviour
    {
        [Header("Grid Settings")] public int gridSize = 10;
        public float gridSpacing = 1f;
        public Material planeMaterial;
        public Material axisMaterial;
        public Material axisHighlightMaterial;
        public Material markerMaterial;

        [Header("Colors")] public Color xColor = Color.red;

        public Color yColor = Color.green;
        public Color zColor = Color.blue;

        private void Start()
        {
            CreateAxisWithMarkers(Vector3.right, xColor, "X-Axis");
            CreateAxisWithMarkers(Vector3.up, yColor, "Y-Axis");
            CreateAxisWithMarkers(Vector3.forward, zColor, "Z-Axis");

            CreatePlane(Vector3.right, Vector3.forward, "Plane_OXY");
            CreatePlane(Vector3.up, Vector3.forward, "Plane_OYZ");
            CreatePlane(Vector3.right, Vector3.up, "Plane_OXZ");
        }

        private void CreateAxisWithMarkers(Vector3 direction, Color color, string name)
        {
            var axis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            axis.name = name;
            axis.transform.SetParent(transform);
            axis.transform.position = new Vector3(0, 0, 0);
            axis.transform.up = direction;
            axis.transform.localScale = new Vector3(0.05f, gridSize, 0.05f);

            var renderer = axis.GetComponent<Renderer>();
            renderer.material = new Material(axisMaterial) { color = color };

            for (var i = -gridSize; i <= gridSize; i++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = name + "_Marker_" + i;
                marker.transform.SetParent(transform);
                marker.transform.position = direction * i;
                marker.transform.localScale = Vector3.one * 0.1f;
                var mr = marker.GetComponent<Renderer>();
                mr.material = new Material(markerMaterial) { color = color };
            }
        }

        private void CreatePlane(Vector3 axis1, Vector3 axis2, string name)
        {
            float extent = gridSize;

            var planeGO = new GameObject(name);
            planeGO.transform.SetParent(transform);

            // Add mesh components
            var filter = planeGO.AddComponent<MeshFilter>();
            var renderer = planeGO.AddComponent<MeshRenderer>();
            var collider = planeGO.AddComponent<BoxCollider>();

            // Create mesh
            var mesh = MeshGenerator.CreatePlaneFacing(axis1, axis2, extent, extent);
            filter.mesh = mesh;

            // Set double-sided material
            var mat = new Material(planeMaterial);
            mat.SetInt("_Cull", (int)CullMode.Off); // hiển thị 2 mặt

// Làm trong suốt
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            var color = mat.color;
            color.a = 0.1f;
            mat.color = color;

            renderer.material = mat;


            // Collider: align with direction
            var normal = Vector3.Cross(axis1, axis2).normalized;
            var size = Mathf.Abs(Vector3.Dot(normal, Vector3.right)) > 0.9f
                ? new Vector3(0.1f, extent * 2, extent * 2)
                : Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.9f
                    ? new Vector3(extent * 2, 0.1f, extent * 2)
                    : new Vector3(extent * 2, extent * 2, 0.1f);
            collider.size = size;
            collider.center = Vector3.zero;
        }


        private void RemoveAllMonoBehaviours(GameObject go)
        {
            var components = go.GetComponents<MonoBehaviour>();
            foreach (var comp in components) Destroy(comp);
        }
    }
}