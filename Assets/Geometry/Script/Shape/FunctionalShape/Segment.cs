// Refactored Segment.cs — ADD DEBUG LOGGING FOR DRAWING FLOW with SNAP POINTS

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

        public void ReconnectFromIds()
        {
            var a = ShapeStorage.GetById(Data.ConnectedPoints[0]) as Point;
            var b = ShapeStorage.GetById(Data.ConnectedPoints[1]) as Point;
            if (a != null && b != null)
            {
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
            ShapeId = data.Id;
            ReconnectFromIds();
        }

        public void SetRaycastIgnore(bool ignore)
        {
            int layer = ignore ? 2 : 0;
            gameObject.layer = layer;

            foreach (Transform child in transform)
                child.gameObject.layer = layer;

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

                pendingStartId = Guid.NewGuid().ToString();
                pendingEndId = Guid.NewGuid().ToString();
                pendingSegId = Guid.NewGuid().ToString();

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

                preview?.SetStartPoint(startPoint);
                preview?.SetEndPoint(endPoint);
                preview?.MarkAsPreview();
                preview?.SetRaycastIgnore(true);
            }

            private static void Update()
            {
                if (current != State.Dragging || startPoint == null || endPoint == null) return;
                if (!PerformDrawing.RaycastMouse(out Vector3 pos)) return;

                Point snap = FindNearbyPoint(pos, exclude: startPoint);
                endPoint.MoveTo((snap != null) ? snap.transform.position : pos);
 
            }

            private static void End()
            {
                if (current != State.Dragging) return;
                
                PerformDrawing.RaycastMouse(out Vector3 pos);
                
                ManipulationManager.Instance.IsDrawing = false;
                current = State.None;
                PerformDrawing.ResetMode();
                
                Point snap = FindNearbyPoint(pos, exclude: startPoint);
                endPoint.MoveTo((snap != null) ? snap.transform.position : pos);

                if (snap != null)
                {
                    preview.SetEndPoint(snap);
                    //endPoint.DestroyShape();
                }

                preview?.SetRaycastIgnore(false);
                startPoint = null;
                endPoint = null;
                preview = null;
                pendingStartId = null;
                pendingEndId = null;
                pendingSegId = null;
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