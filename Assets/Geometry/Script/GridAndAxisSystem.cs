using UnityEngine;

namespace Manipulator
{
    public class GridAndAxisSystem : MonoBehaviour
    {
        [Header("Grid Settings")] public int gridSize = 20;
        public float gridSpacing = 1f;
        public Color primaryGridColor = Color.gray;
        public Color secondaryGridColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

        [Header("Axis Settings")] public float axisLength = 10f;
        public float axisThickness = 0.1f;
        public Color xAxisColor = Color.red;
        public Color yAxisColor = Color.green;
        public Color zAxisColor = Color.blue;

        public Material defaultMaterial;
        public Material highlightMaterial;

        private Material lineMaterial;

        void OnDrawGizmos()
        {
            DrawGridGizmos();
            DrawAxesGizmos();
        }

        void DrawGridGizmos()
        {
            for (int i = -gridSize; i <= gridSize; i++)
            {
                Gizmos.color = (i % 5 == 0) ? primaryGridColor : secondaryGridColor;

                Gizmos.DrawLine(new Vector3(-gridSize * gridSpacing, 0, i * gridSpacing),
                                new Vector3(gridSize * gridSpacing, 0, i * gridSpacing));

                Gizmos.DrawLine(new Vector3(i * gridSpacing, 0, -gridSize * gridSpacing),
                                new Vector3(i * gridSpacing, 0, gridSize * gridSpacing));
            }
        }

        void DrawAxesGizmos()
        {
            Gizmos.color = xAxisColor;
            Gizmos.DrawLine(Vector3.zero, Vector3.right * axisLength);

            Gizmos.color = yAxisColor;
            Gizmos.DrawLine(Vector3.zero, Vector3.up * axisLength);

            Gizmos.color = zAxisColor;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * axisLength);
        }

        void Start()
        {
            DrawAxisMarkers();
            CreateGridPlane();
            CreateLineMaterial();
        }

        void DrawAxisMarkers()
        {
            for (int i = 1; i <= axisLength; i++)
            {
                CreatePoint(new Vector3(i, 0, 0), xAxisColor, $"X-{i}");
                CreatePoint(new Vector3(0, i, 0), yAxisColor, $"Y-{i}");
                CreatePoint(new Vector3(0, 0, i), zAxisColor, $"Z-{i}");
            }
        }

        void CreatePoint(Vector3 pos, Color color, string name)
        {
            var point = ShapeFactory.CreateShape("Point", pos);
            point.name = name;
            var renderer = point.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = new Material(defaultMaterial);
                renderer.material.color = color;
            }
        }

        void CreateGridPlane()
        {
            var p1 = (Point)ShapeFactory.CreateShape("Point", new Vector3(1, 0, 0));
            var p2 = (Point)ShapeFactory.CreateShape("Point", new Vector3(0, 0, 1));
            var p3 = (Point)ShapeFactory.CreateShape("Point", new Vector3(0, 0, 0));

            var plane = (PlaneShape)ShapeFactory.CreateShape("Plane", Vector3.zero);
            plane.AddPivot(p1);
            plane.AddPivot(p2);
            plane.AddPivot(p3);
            plane.CompleteDraw();
        }

        void OnRenderObject()
        {
            if (!lineMaterial) CreateLineMaterial();

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);
            DrawGrid();
            GL.PopMatrix();
        }

        void CreateLineMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            lineMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        }

        void DrawGrid()
        {
            GL.Begin(GL.LINES);
            for (int i = -gridSize; i <= gridSize; i++)
            {
                GL.Color((i % 5 == 0) ? primaryGridColor : secondaryGridColor);
                GL.Vertex(new Vector3(-gridSize * gridSpacing, 0, i * gridSpacing));
                GL.Vertex(new Vector3(gridSize * gridSpacing, 0, i * gridSpacing));
                GL.Vertex(new Vector3(i * gridSpacing, 0, -gridSize * gridSpacing));
                GL.Vertex(new Vector3(i * gridSpacing, 0, gridSize * gridSpacing));
            }
            GL.End();
        }
    }
}
