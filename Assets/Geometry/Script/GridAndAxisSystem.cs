using UnityEngine;
using UnityEngine.Rendering;

namespace Manipulator
{
    public class GridAndAxisSystem : MonoBehaviour
    {
        [Header("Grid Settings")] public int gridSize;
        public float gridSpacing = 1f;
        public Material planeMaterial;
        public Material axisMaterial;
        public Material axisHighlightMaterial;
        public GameObject cameraTarget;

        [Header("Colors")] public Color xColor = Color.red;

        public Color yColor = Color.green;
        public Color zColor = Color.blue;

        private void Start()
        {
            CreateAxisWithMarkers(Vector3.right, xColor, "X-Axis");
            CreateAxisWithMarkers(Vector3.up, yColor, "Y-Axis");
            CreateAxisWithMarkers(Vector3.forward, zColor, "Z-Axis");

            CreatePlane(Vector3.right, Vector3.forward, "Plane_OXZ", render: false);
            
            
            CreatePlane(Vector3.up, Vector3.forward, "Plane_OYZ", new Vector3(-gridSize, 0, 0));
            CreatePlane(Vector3.right, Vector3.up, "Plane_OXY", new Vector3(0, 0, -gridSize));
            CreatePlane(Vector3.up, Vector3.forward, "Plane_OYZ", new Vector3(gridSize, 0, 0));
            CreatePlane(Vector3.right, Vector3.up, "Plane_OXY",  new Vector3(0, 0, gridSize));
            
            
        }

        private void CreateAxisWithMarkers(Vector3 direction, Color color, string name)
        {
            var axis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            axis.name = name;
            axis.transform.SetParent(transform);
            axis.transform.position = cameraTarget.transform.position;
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
                mr.material = new Material(axisMaterial) { color = color };
            }
        }

        private void CreatePlane(Vector3 axis1, Vector3 axis2, string name, Vector3 offset = new Vector3(), bool render = true)
        {
            float extent = gridSize;

            var planeGO = new GameObject(name);
            planeGO.transform.position = offset + cameraTarget.transform.position;
            planeGO.transform.SetParent(transform);

            // Add mesh components
            var filter = planeGO.AddComponent<MeshFilter>();
            var collider = planeGO.AddComponent<BoxCollider>();
            var mesh = MeshGenerator.CreatePlaneFacing(axis1, axis2, extent, extent);
            filter.mesh = mesh;
            
            if (render)
            {
                var renderer = planeGO.AddComponent<MeshRenderer>();
                // Create mesh

                // Set double-sided material
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.CopyPropertiesFromMaterial(planeMaterial);
// Apply your transparency/double-sided setup as before
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;

                var color = mat.color;
                color.a = 0.3f;
                mat.color = color;

                renderer.material = mat;


            }

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