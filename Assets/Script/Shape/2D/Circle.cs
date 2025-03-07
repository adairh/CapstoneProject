using UnityEngine;

public class Circle : CircularShape, IDrawable2D
{
    public float Radius { get; set; }
    private const int SEGMENTS = 36; 
    private GameObject[] edges;

    public Circle(Vector3 center, float radius) : base(center, "Circle")
    {
        Radius = radius;
        SetupGameObject();
    }

    private void SetupGameObject()
    {
        GO = new GameObject(Name);
        GO.transform.position = Position;

        // ✅ Add interactive components
        /*GO.AddComponent<DraggableShape>();
        GO.AddComponent<HoverableShape>().SetMaterials(DefaultMaterial, HighlightMaterial);
        GO.AddComponent<ScalableShape>();
        GO.AddComponent<RotatableShape>();*/
        
        // ✅ Add a Collider (for interactions)
        CircleCollider2D collider = GO.AddComponent<CircleCollider2D>();
        collider.radius = Radius; 
        collider.offset = Vector2.zero;
        edges = new GameObject[SEGMENTS];
        Draw2D();
    }

    public void Draw2D()
    {
        float angleStep = 360f / SEGMENTS;
        Vector3 prevPoint = GetPointOnCircle(0);

        for (int i = 1; i <= SEGMENTS; i++)
        {
            Vector3 nextPoint = GetPointOnCircle(i * angleStep);
            CreateEdge(i - 1, prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }

    private Vector3 GetPointOnCircle(float angleDegrees)
    {
        float rad = Mathf.Deg2Rad * angleDegrees;
    
        // Get the camera-aligned plane
        Vector3 forward = Camera.main.transform.forward;  // Camera looking direction
        Vector3 right = Camera.main.transform.right;  // Horizontal axis
        Vector3 up = Vector3.Cross(forward, right);  // Vertical axis

        return Position + (right * Radius * Mathf.Cos(rad)) + (up * Radius * Mathf.Sin(rad));
    }

    private void CreateEdge(int index, Vector3 start, Vector3 end)
    {
        if (edges[index] == null)
        {
            edges[index] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            edges[index].transform.SetParent(GO.transform);
            edges[index].GetComponent<Renderer>().material = DefaultMaterial;
        }

        Vector3 midPoint = (start + end) / 2;
        float length = Vector3.Distance(start, end);

        edges[index].transform.position = midPoint;
        edges[index].transform.localScale = new Vector3(0.05f, length / 2, 0.05f);
        edges[index].transform.rotation = Quaternion.FromToRotation(Vector3.up, end - start);
    }


    public override void Draw()
    {
        // ✅ Add a Collider (for interactions)
        CircleCollider2D collider = GO.GetComponent<CircleCollider2D>();
        collider.radius = Radius; 
        collider.offset = Vector2.zero;
        
        float angleStep = 360f / SEGMENTS;
        Vector3 prevPoint = GetPointOnCircle(0);

        for (int i = 1; i <= SEGMENTS; i++)
        {
            Vector3 nextPoint = GetPointOnCircle(i * angleStep);
            
            Vector3 midPoint = (prevPoint + nextPoint) / 2; 
            float length = Vector3.Distance(prevPoint, nextPoint);

            edges[i - 1].transform.position = midPoint;
            edges[i - 1].transform.localScale = new Vector3(0.05f, length / 2, 0.05f);
            edges[i - 1].transform.rotation = Quaternion.FromToRotation(Vector3.up, nextPoint - prevPoint);  
                
            prevPoint = nextPoint;
        } 
    }
    protected override void InitializeSettings()
    {
        //throw new System.NotImplementedException();
    }

    public override void OpenConfigPanel()
    {
        //throw new System.NotImplementedException();
    }

    private static bool drawing = false;
    private static Vector3 startPoint;
    private static Circle circle;
    private static Vector3 startScreenPoint;
    public static void Sketch(Vector3 vector3, Vector3 screenPoint, Camera mainCamera)
    {
        
        if (Input.GetMouseButtonDown(0)) // Click to start
        {
            if (!drawing)
            { 
                startPoint = vector3;
                startScreenPoint = screenPoint;
                circle = new Circle(startPoint, 0);
                drawing = true;
            }
        }
        else if (drawing && Input.GetMouseButton(0)) // Hold to resize
        { 
            float newRadius = Vector3.Distance(startScreenPoint, screenPoint)/100;
            if (!Mathf.Approximately(circle.Radius, newRadius)) // Prevent redundant updates
            {
                circle.Radius = newRadius;
                circle.GO.transform.rotation = GetAlignedRotation(mainCamera); // Rotate based on camera
                circle.Draw();
            }
        }
        else if (Input.GetMouseButtonUp(0)) // Release to finalize
        {
            drawing = false;
        }
    }
}
