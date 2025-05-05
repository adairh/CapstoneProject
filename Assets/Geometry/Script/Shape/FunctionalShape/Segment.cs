using UnityEngine; 

namespace Manipulator
{
    public class Segment : Shape
    {
        public Point StartPoint { get; private set; }
        public Point EndPoint { get; private set; }

        private GameObject visual;

        #region INIT

        protected override void Awake()
        {
            base.Awake();
            visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.transform.SetParent(transform);
            visual.GetComponent<Renderer>().material = MaterialLibrary.Get(MaterialType.Default);
            DestroyImmediate(visual.GetComponent<Collider>());
        }

        public void SetEndpoints(Point a, Point b)
        {
            StartPoint = a;
            EndPoint = b;

            AddPivot(a);
            AddPivot(b);

            UpdateVisual();
        }

        #endregion

        #region UPDATE

        public override void CompleteDraw()
        {
            base.CompleteDraw();
            UpdateVisual();
        }

        public override void UpdateHitbox()
        {
            // Optional: Add mesh collider based on cylinder
        }

        private void UpdateVisual()
        {
            if (StartPoint == null || EndPoint == null || visual == null)
                return;

            Vector3 a = StartPoint.transform.position;
            Vector3 b = EndPoint.transform.position;
            Vector3 mid = (a + b) / 2;
            Vector3 dir = b - a;

            float length = dir.magnitude;

            visual.transform.position = mid;
            visual.transform.rotation = Quaternion.LookRotation(dir);
            visual.transform.Rotate(90, 0, 0); // Align cylinder along Z
            visual.transform.localScale = new Vector3(0.05f, length / 2f, 0.05f);
        }

        protected override void OnPivotChanged(Point pt)
        {
            base.OnPivotChanged(pt);
            UpdateVisual();
        }

        public void ReconnectFromIds()
        {
            var a = ShapeStorage.GetById(Data.ConnectedPoints[0]) as Point;
            var b = ShapeStorage.GetById(Data.ConnectedPoints[1]) as Point;;
            if (a != null && b != null)
                SetEndpoints(a, b);
        }


        #endregion

        #region SERIALIZATION

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "Segment";
            data.ConnectedPoints = new System.Collections.Generic.List<string>
            {
                StartPoint.ShapeId,
                EndPoint.ShapeId
            };
            return data;
        }

        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);
            ReconnectFromIds();
            // Connect points later via factory or controller
        }

        #endregion
    }
}