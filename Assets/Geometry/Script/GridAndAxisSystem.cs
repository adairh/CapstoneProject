using UnityEngine;

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

        [Header("Colors")]
        public Color xColor = Color.red;
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

        void CreateAxisWithMarkers(Vector3 direction, Color color, string name)
        {
            var axis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            axis.name = name;
            axis.transform.SetParent(transform);
            axis.transform.position = new Vector3(0, 0, 0);
            axis.transform.up = direction;
            axis.transform.localScale = new Vector3(0.05f, gridSize, 0.05f);

            var renderer = axis.GetComponent<Renderer>();
            renderer.material = new Material(axisMaterial) { color = color };

            for (int i = -gridSize; i <= gridSize; i++)
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

        void CreatePlane(Vector3 axis1, Vector3 axis2, string name)
        {
            Vector3 p1 = Vector3.zero;
            Vector3 p2 = axis1 * gridSize;
            Vector3 p3 = axis2 * gridSize;

            var point1 = (Point)ShapeFactory.CreateShape("Point", p1);
            var point2 = (Point)ShapeFactory.CreateShape("Point", p2);
            var point3 = (Point)ShapeFactory.CreateShape("Point", p3);

            var plane = (PlaneShape)ShapeFactory.CreateShape("Plane", Vector3.zero);
            plane.name = name;
            plane.AddPivot(point1);
            plane.AddPivot(point2);
            plane.AddPivot(point3);
            plane.CompleteDraw();

            var renderer = plane.GetComponent<MeshRenderer>();
            if (renderer)
            {
                renderer.material = new Material(planeMaterial);
            }
        }
    }
}
