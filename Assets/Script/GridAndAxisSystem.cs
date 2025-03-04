using UnityEngine;
using UnityEditor;  // Required for Handles

public class GridAndAxisSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSize = 20; // Total grid size
    public float gridSpacing = 1f; // Space between grid lines
    public Color primaryGridColor = Color.gray; // Color for primary lines
    public Color secondaryGridColor = new Color(0.8f, 0.8f, 0.8f, 0.5f); // Lighter color for secondary lines

    [Header("Axis Settings")]
    public float axisLength = 10f;
    public float axisThickness = 0.1f; 
    public Color xAxisColor = Color.red;
    public Color yAxisColor = Color.green;
    public Color zAxisColor = Color.blue;
    
    public Material defaultMaterial;
    public Material highlightMaterial; 

    private Material lineMaterial;
    private GameObject xAxis, yAxis, zAxis;

    void OnDrawGizmos()
    {
        DrawGridGizmos();
        DrawAxesGizmos();
        DrawAxisLabels();
    }

    void DrawGridGizmos()
    {
        for (int i = -gridSize; i <= gridSize; i++)
        {
            Gizmos.color = (i % 5 == 0) ? primaryGridColor : secondaryGridColor;
            
            // Lines parallel to X-axis
            Gizmos.DrawLine(new Vector3(-gridSize * gridSpacing, 0, i * gridSpacing),
                            new Vector3(gridSize * gridSpacing, 0, i * gridSpacing));

            // Lines parallel to Z-axis
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

    void DrawAxisLabels()
    {
        GUIStyle labelStyle = new GUIStyle { normal = { textColor = Color.black }, fontSize = 12, alignment = TextAnchor.MiddleCenter };
        
        for (int i = 1; i <= axisLength; i++)
        {
            Handles.Label(new Vector3(i, 0.1f, 0.1f), i.ToString(), labelStyle);
            Handles.Label(new Vector3(0.1f, i, 0.1f), i.ToString(), labelStyle);
            Handles.Label(new Vector3(0.1f, 0.1f, i), i.ToString(), labelStyle);
        }
    }

    void Start()
    {
        DrawPlane();
        DrawAxisLabelsPivot();
        CreateLineMaterial();
        DrawAxes();
    }

    void DrawPlane()
    {
        Vector3[][] planePoints = new Vector3[][]
        {
            new Vector3[] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0) },
            new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) },
            new Vector3[] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1) }
        };

        foreach (var points in planePoints)
        {
            GameObject go = new GameObject();
            new Plane(new Point[]
            {
                new Point(points[0], go),
                new Point(points[1], go),
                new Point(points[2], go)
            }, gridSize, go);
        }
    }
    
// Draw labels for each axis at specified intervals
    void DrawAxisLabelsPivot()
    {
        // Draw labels on X-axis
        for (int i = 1; i <= axisLength; i++)
        {
            CreatePoint(new Vector3(i, 0, 0), Color.red, "Label-X-" + i); 
        }

        // Draw labels on Y-axis
        for (int i = 1; i <= axisLength; i++)
        {
            GameObject go = CreatePoint(new Vector3(0, i, 0), Color.green, "Label-Y-" + i);
            go.transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        // Draw labels on Z-axis
        for (int i = 1; i <= axisLength; i++)
        {
            CreatePoint(new Vector3(0, 0, i), Color.blue, "Label-Z-" + i); 
        }
    }
    
    GameObject CreatePoint(Vector3 pos, Color color, string name)
    {
        GameObject axis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        axis.name = name;
        axis.transform.parent = this.transform;

        // Position and scale the axis
        axis.transform.localScale = Vector3.one * (0.1f);
        axis.transform.position = pos; 

        // Apply material
        Renderer renderer = axis.GetComponent<Renderer>();
        renderer.material = new Material(defaultMaterial);
        renderer.material.color = color;

        // Add hover interaction
        AxisHover hoverScript = axis.AddComponent<AxisHover>();
        hoverScript.defaultMaterial = renderer.material;
        hoverScript.highlightMaterial = highlightMaterial;

        return axis;
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
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        //lineMaterial.renderQueue = 2000; // Default opaque queue
        lineMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        lineMaterial.SetInt("_ZWrite", 0);
        lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        //lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
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

    void DrawAxes()
    {
        xAxis = CreateAxis(Vector3.right * axisLength, xAxisColor, "X-Axis");
        yAxis = CreateAxis(Vector3.up * axisLength, yAxisColor, "Y-Axis");
        zAxis = CreateAxis(Vector3.forward * axisLength, zAxisColor, "Z-Axis");
    }

    GameObject CreateAxis(Vector3 direction, Color color, string name)
    {
        GameObject axis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        axis.name = name;
        axis.transform.parent = this.transform;
        axis.transform.localScale = new Vector3(axisThickness, direction.magnitude / 2, axisThickness);
        axis.transform.position = direction / 2;
        axis.transform.up = direction.normalized;

        Renderer renderer = axis.GetComponent<Renderer>();
        renderer.material = new Material(defaultMaterial) { color = color };

        AxisHover hoverScript = axis.AddComponent<AxisHover>();
        hoverScript.defaultMaterial = renderer.material;
        hoverScript.highlightMaterial = highlightMaterial;

        return axis;
    }

    public class AxisHover : MonoBehaviour
    {
        public Material defaultMaterial;
        public Material highlightMaterial;

        void OnMouseEnter()
        {
            GetComponent<Renderer>().material = highlightMaterial;
            GetComponent<Renderer>().material.color = defaultMaterial.color;
        }

        void OnMouseExit()
        {
            GetComponent<Renderer>().material = defaultMaterial;
        }
    }
}
