// Refactored Segment.cs — ADD DEBUG LOGGING FOR DRAWING FLOW with SNAP POINTS and UNDO-SAFE SPAWN + RECONNECT SYNC FIX

using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Manipulator
{
    public class Segment : Shape
    {
        public Point StartPoint { get; private set; }
        public Point EndPoint { get; private set; }

        private GameObject visual;
        private bool isPreview = false;

        protected override void Awake()
        {
            base.Awake();
            visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.transform.SetParent(transform);
            visual.GetComponent<Renderer>().material = MaterialLibrary.Get(MaterialType.Default);
            DestroyImmediate(visual.GetComponent<Collider>());
        }

        private void Update()
        {
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
        }

        public void MarkAsPreview() => isPreview = true;

        public void SetStartPoint(Point a)
        {
            StartPoint = a;
            AddPivot(a);
        }

        public void SetEndPoint(Point b)
        {
            EndPoint = b;
            AddPivot(b);
            UpdateVisual();
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
        }

        protected override void OnPivotChanged(Point pt)
        {
            base.OnPivotChanged(pt);
            UpdateVisual();
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
            ShapeId = data.Id;
            StartCoroutine(WaitAndReconnect(data.ConnectedPoints[0], data.ConnectedPoints[1]));
        }

        private IEnumerator WaitAndReconnect(string aId, string bId)
        {
            while (ShapeStorage.GetById(aId) == null || ShapeStorage.GetById(bId) == null)
                yield return null;

            var a = ShapeStorage.GetById(aId) as Point;
            var b = ShapeStorage.GetById(bId) as Point;
            if (a != null && b != null)
            {
                SetStartPoint(a);
                SetEndPoint(b);
            }
        }

        public void SetRaycastIgnore(bool ignore)
        {
            int layer = ignore ? 2 : 0;
            gameObject.layer = layer;

            foreach (Transform child in transform)
                child.gameObject.layer = layer;

            if (StartPoint != null) StartPoint.gameObject.layer = layer;
            if (EndPoint != null) EndPoint.gameObject.layer = layer;
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

            private const float SnapDistance = 0.3f;

            public static void UpdateSegmentInput()
            {
                if (Input.GetMouseButtonDown(0)) Start();
                else if (Input.GetMouseButton(0)) Update();
                else if (Input.GetMouseButtonUp(0)) End();
            }

            private static void Start()
            {
                if (!PerformDrawing.RaycastMouse(out Vector3 pos)) return;
                var mm = ManipulationManager.Instance;
                if (mm.IsDrawing) return;
                mm.IsDrawing = true;

                Point snap = FindNearbyPoint(pos);
                if (snap != null)
                {
                    startPoint = snap;
                    pendingStartId = snap.ShapeId;
                }
                else
                {
                    pendingStartId = Guid.NewGuid().ToString();
                    var data = new ShapeData { Id = pendingStartId, Type = "Point", Position = pos, Rotation = Quaternion.identity, Scale = Vector3.one, ConnectedPoints = new(), Settings = new() };
                    UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeAction(data));
                }

                pendingEndId = Guid.NewGuid().ToString();
                pendingSegId = Guid.NewGuid().ToString();

                var p2Data = new ShapeData { Id = pendingEndId, Type = "Point", Position = pos, Rotation = Quaternion.identity, Scale = Vector3.one, ConnectedPoints = new(), Settings = new() };
                
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeAction(p2Data));
                
                var segData = new ShapeData { Id = pendingSegId, Type = "Segment", Position = pos, Rotation = Quaternion.identity, Scale = Vector3.one, ConnectedPoints = new() { pendingStartId, pendingEndId }, Settings = new() };

                UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeAction(segData));

                current = State.Dragging;  
            }

            private static void Update()
            {
                if (current != State.Dragging || startPoint == null || endPoint == null) return;
                if (!PerformDrawing.RaycastMouse(out Vector3 pos)) return;

                Point snap = FindNearbyPoint(pos, exclude: startPoint);
                endPoint.MoveTo((snap != null) ? snap.transform.position : pos, queue: false);

                if (snap != null)
                    preview.SetEndPoint(snap);
            }

            private static void End()
            {
                if (current != State.Dragging) return;
                ManipulationManager.Instance.IsDrawing = false;
                current = State.None;
                PerformDrawing.ResetMode();

                if (preview != null) preview.SetRaycastIgnore(false);
                startPoint = null;
                endPoint = null;
                preview = null;
                pendingStartId = null;
                pendingEndId = null;
                pendingSegId = null;
            }

            public static void OnStartPointReady(Point p)
            {/*
                Debug.LogError($"[OnStartPointReady] Called with Point ID = {p.ShapeId}");
                Debug.LogError($"[OnStartPointReady] Current pendingStartId = {pendingStartId}, pendingEndId = {pendingEndId}");
                */

                if (p.ShapeId == pendingStartId)
                {
                    startPoint = p;
                    //Debug.LogError("[OnStartPointReady] Assigned as StartPoint");
                }
                else if (p.ShapeId == pendingEndId)
                {
                    endPoint = p;
                    //Debug.LogError("[OnStartPointReady] Assigned as EndPoint");
                }
                else
                {
                    //Debug.LogError("[OnStartPointReady] Point ID does not match any pending ID");
                }

                if (preview != null)
                {
                    //Debug.LogError("[OnStartPointReady] Preview exists, attempting to assign points");

                    if (startPoint != null)
                    {
                        preview.SetStartPoint(startPoint);
                        //Debug.LogError($"[OnStartPointReady] Set StartPoint for preview segment to {startPoint.ShapeId}");
                    }

                    if (endPoint != null)
                    {
                        preview.SetEndPoint(endPoint);
                        //Debug.LogError($"[OnStartPointReady] Set EndPoint for preview segment to {endPoint.ShapeId}");
                    }
                }
                else
                {
                    //Debug.LogError("[OnStartPointReady] Preview is still null, waiting for OnSegmentReady");
                }
            }


            public static void OnSegmentReady(Segment s)
            {
                //Debug.LogError($"[OnSegmentReady] Incoming segment ID = {s.ShapeId}, Expected = {pendingSegId}");

                if (s.ShapeId != pendingSegId)
                {
                    //Debug.LogError($"[OnSegmentReady] Skipped: Mismatched ID (got {s.ShapeId}, expected {pendingSegId})");
                    return;
                }

                preview = s;
                //Debug.LogError($"[OnSegmentReady] Assigned preview segment (ID = {preview.ShapeId})");

                if (startPoint != null)
                {
                    preview.SetStartPoint(startPoint);
                    //Debug.LogError($"[OnSegmentReady] StartPoint set to ID = {startPoint.ShapeId}");
                }
                else
                {
                    //Debug.LogError("[OnSegmentReady] StartPoint is null");
                }

                if (endPoint != null)
                {
                    preview.SetEndPoint(endPoint);
                    //Debug.LogError($"[OnSegmentReady] EndPoint set to ID = {endPoint.ShapeId}");
                }
                else
                {
                    //Debug.LogError("[OnSegmentReady] EndPoint is null");
                }

                preview.MarkAsPreview();
                preview.SetRaycastIgnore(true);
                //Debug.LogError("[OnSegmentReady] Marked as preview and raycast ignored");
            }


            private static Point FindNearbyPoint(Vector3 pos, Point exclude = null)
            {
                foreach (var shape in ShapeStorage.GetAllShapes())
                {
                    if (shape is Point pt && pt != exclude)
                    {
                        if (Vector3.Distance(pos, pt.transform.position) < SnapDistance)
                            return pt;
                    }
                }
                return null;
            }
        }
    }
}
