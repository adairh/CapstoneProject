// Refactored Segment.cs — ADD DEBUG LOGGING FOR DRAWING FLOW

using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

namespace Manipulator
{
    public class Segment : Shape
    {
        public Point StartPoint { get; private set; }
        public Point EndPoint { get; private set; }

        private GameObject visual;
        private bool isPreview = false;
        private NetworkPositionSync positionSync;

        protected override void Awake()
        {
            base.Awake();
            visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.transform.SetParent(transform);
            visual.GetComponent<Renderer>().material = MaterialLibrary.Get(MaterialType.Default); 
            
            if (!TryGetComponent(out positionSync))
                positionSync = gameObject.AddComponent<NetworkPositionSync>();
            //DestroyImmediate(visual.GetComponent<Collider>());
            ShapeStorage.Register(this);
        }

        private void Update()
        {
            Debug.Log($"[Segment.Update] SEG ID={ShapeId}, isPreview={isPreview}, Start={StartPoint != null}, End={EndPoint != null}");
            if (!isPreview || StartPoint == null || visual == null) return;

            Vector3 a = StartPoint.transform.position;
            Vector3 b = EndPoint != null ? EndPoint.transform.position : a;
            Vector3 mid = (a + b) / 2;
            Vector3 dir = b - a;
            float length = dir.magnitude;

            visual.transform.position = mid;
            visual.transform.rotation = Quaternion.LookRotation(dir);
            visual.transform.Rotate(90, 0, 0);
            visual.transform.localScale = new Vector3(0.05f, length / 2f, 0.05f);

            Debug.Log($"[Segment.Update] visual.position={visual.transform.position}, scale={visual.transform.localScale}");
        }

        public void MarkAsPreview()
        {
            isPreview = true;
            Debug.Log($"[Segment.MarkAsPreview] Called on segment ID = {ShapeId}");
        }

        public void SetStartPoint(Point a)
        {
            StartPoint = a;
            AddPivot(a);
            Debug.Log($"[Segment.SetStartPoint] Segment {ShapeId} -> StartPoint = {a.ShapeId}");
        }

        public void SetEndPoint(Point b)
        {
            EndPoint = b;
            AddPivot(b);
            UpdateVisual();
            Debug.Log($"[Segment.SetEndPoint] Segment {ShapeId} -> EndPoint = {b.ShapeId}");
        }


        private void UpdateVisual()
        {
            if (StartPoint == null || EndPoint == null || visual == null) return;

            Vector3 a = StartPoint.transform.position;
            Vector3 b = EndPoint.transform.position;
            Vector3 mid = (a + b) / 2;
            Vector3 dir = b - a;
            float length = dir.magnitude;

            visual.transform.position = mid;
            visual.transform.rotation = Quaternion.LookRotation(dir);
            visual.transform.Rotate(90, 0, 0);
            visual.transform.localScale = new Vector3(0.05f, length / 2f, 0.05f);

            Debug.Log($"[Segment.UpdateVisual] updated segment between {a} and {b}, scale={length}");
        }

        protected override void OnPivotChanged(Point pt)
        {
            base.OnPivotChanged(pt);
            UpdateVisual();
        }

        public void ReconnectFromIds()
        {
            var a = ShapeStorage.GetById(Data.ConnectedPoints[0]) as Point;
            var b = ShapeStorage.GetById(Data.ConnectedPoints[1]) as Point;
            if (a != null && b != null) {
                SetStartPoint(a);  
                SetEndPoint(b);  
            }
        }

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "Segment";
            data.ConnectedPoints = new List<string>
            {
                StartPoint.ShapeId,
                EndPoint.ShapeId
            };
            return data;
        }

        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);
            ShapeId = data.Id; // Ensure ID consistency across network
            ReconnectFromIds();
        }
        
        public void SetRaycastIgnore(bool ignore)
        {
            int layer = ignore ? 2 : 0;
            gameObject.layer = layer;

            foreach (Transform child in transform)
            {
                child.gameObject.layer = layer;
            }

            StartPoint.gameObject.layer = layer;
            EndPoint.gameObject.layer = layer;
        }


        public static class Drawer
        {
            private static Point startPoint;
            private static Point endPoint;
            private static Segment preview;
            private static string pendingStartId;
            private static string pendingEndId;
            private static string pendingSegId;

            private enum State { None, Dragging }
            private static State current = State.None;

            public static void UpdateSegmentInput()
            {
                var mm = ManipulationManager.Instance;
                Debug.Log($"[Drawer.Update] frame={Time.frameCount}, state={current}");
                if (Input.GetMouseButtonDown(0) && !mm.IsDrawing) Start();
                else if (Input.GetMouseButton(0) && mm.IsDrawing) Update();
                else if (Input.GetMouseButtonUp(0) && mm.IsDrawing) End();
            }

            private static void Start()
            {
                if (!PerformDrawing.RaycastMouse(out Vector3 pos)) return;
                var mm = ManipulationManager.Instance;
                if (mm.IsDrawing) return;
                mm.IsDrawing = true;

                pendingStartId = Guid.NewGuid().ToString();
                pendingEndId = Guid.NewGuid().ToString();
                pendingSegId = Guid.NewGuid().ToString();

                Debug.Log($"[Drawer.Start] Begin draw at pos={pos}, pendingSegId={pendingSegId}");

                NetworkShapeSpawner.Instance.CreateShapeNetworked(new ShapeData
                {
                    Id = pendingStartId,
                    Type = "Point",
                    Position = pos,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new(),
                    Settings = new()
                }, out Shape p1);

                NetworkShapeSpawner.Instance.CreateShapeNetworked(new ShapeData
                {
                    Id = pendingEndId,
                    Type = "Point",
                    Position = pos,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new(),
                    Settings = new()
                }, out Shape p2);

                NetworkShapeSpawner.Instance.CreateShapeNetworked(new ShapeData
                {
                    Id = pendingSegId,
                    Type = "Segment",
                    Position = pos,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new() { pendingStartId, pendingEndId },
                    Settings = new()
                }, out Shape seg);

                startPoint = p1 as Point;
                endPoint = p2 as Point;
                preview = seg as Segment;
                current = State.Dragging;

                Debug.Log($"[Drawer.Start] Preview SegID={preview?.ShapeId}, StartID={startPoint?.ShapeId}, EndID={endPoint?.ShapeId}");

                preview?.SetStartPoint(startPoint);
                preview?.SetEndPoint(endPoint);
                preview?.MarkAsPreview();
                preview?.SetRaycastIgnore(true);

                
            }

            private static void Update()
            {
                Debug.Log($"[Drawer.Drag] isPreview={preview!= null}, Start={startPoint != null}, End={endPoint != null}");
                if (current != State.Dragging || startPoint == null || endPoint == null) return;
                if (!PerformDrawing.RaycastMouse(out Vector3 pos)) return;

                Debug.Log($"[Drawer.Update] Dragging segment, moving endpoint to {pos}");
                endPoint.MoveTo(pos, true);
                //preview.SetEndPoint(endPoint);
            }

            private static void End()
            {
                if (current != State.Dragging) return;

                Debug.Log("[Drawer.End] Finished drawing");
                ManipulationManager.Instance.IsDrawing = false;
                current = State.None;
                PerformDrawing.ResetMode();

                preview?.SetRaycastIgnore(false);
                startPoint = null;
                endPoint = null;
                preview = null;
                pendingStartId = null;
                pendingEndId = null;
                pendingSegId = null;
            }

             

            
            
            public static void OnStartPointReady(Point p)
            {
                Debug.Log($"[Drawer.OnStartPointReady] id={p.ShapeId}, segId={pendingSegId}");

                if (p.ShapeId == pendingStartId)
                    startPoint = p;
                else if (p.ShapeId == pendingEndId)
                    endPoint = p;

                if (startPoint != null && endPoint != null && preview == null)
                {
                    preview = ShapeStorage.GetById(pendingSegId) as Segment;
                    Debug.Log($"[Drawer.OnStartPointReady] found preview segment: {preview != null}");
                    if (preview != null)
                    {
                        preview.SetStartPoint(startPoint);
                        preview.SetEndPoint(endPoint);
                        preview.MarkAsPreview();
                    }
                }
            }
        }
    }
}