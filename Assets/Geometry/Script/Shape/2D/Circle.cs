using UnityEngine;

public class Circle : CircularShape, IDrawable2D
{ 
    private const int SEGMENTS = 36; 
    private GameObject[] edges;

    public Circle(Vector3 center, float radius, Shape parent) : base(center, "Circle", parent)
    {
        Radius = radius;
        SetupGameObject();
    }
    
    public Circle(Vector3 center, float radius) : this(center, radius, null){
    }

    protected override void SetupGameObject()
    {
        GO = new GameObject(Name);
        GO.transform.position = Position;

        edges = new GameObject[SEGMENTS]; // ✅ Initialize edges array before calling Draw2D()
    
        Draw2D(); // ✅ Call after initializing edges
        UpdateHitbox();
        base.SetupGameObject();
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

        // ✅ Use the object's local right and up directions
        Vector3 right = GO.transform.right; // Correctly rotated X-axis
        Vector3 up = GO.transform.up; // Correctly rotated Y-axis

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

        // ✅ Ensure correct rotation using local transform axes
        edges[index].transform.rotation = Quaternion.LookRotation(GO.transform.forward, end - start);
    }



 
 

    public override void Drawing()
    { 
        
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

    public override GameObject[] Components()
    {
        return edges;
    }

    protected override void InitializeSettings()
    { 
        AppendSettings(
            new RadiusSetting(Radius, this),
            new PositionSetting(Position, this)
        );
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
            float newRadius = Vector3.Distance(startScreenPoint, screenPoint) / 100;
            if (!Mathf.Approximately(circle.Radius, newRadius)) 
            {
                circle.Radius = newRadius;
                circle.GO.transform.rotation = GetAlignedRotation(mainCamera);
                //circle.UpdateHitbox(); // ✅ Ensure this is called after setting up the shape
                circle.Draw();
            }
        }

        else if (Input.GetMouseButtonUp(0)) // Release to finalize
        {
            if (drawing)
            {
                drawing = false;
                circle.CompleteDraw();
            }
        }
    }
    public override void UpdateHitbox()
    {
        // ✅ Ensure GO is not null
        if (GO == null)
        {
            Debug.LogError("❌ UpdateHitbox() called before GameObject was initialized.");
            return;
        }

        // ✅ Ensure the collider exists
        SphereCollider sphereCollider = GO.GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = GO.AddComponent<SphereCollider>();
        }

        // ✅ Set the correct radius
        sphereCollider.radius = Radius;

        // ✅ Match the rotation of the shape
        sphereCollider.transform.rotation = GO.transform.rotation;
    }



    public override void CompleteDraw()
    {
        UpdateHitbox();
        base.CompleteDraw();
    }
}
