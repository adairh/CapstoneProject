using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Manipulator
{
    public class Segment : Shape
    {
        private NetworkPositionSync positionSync;
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

            if (!TryGetComponent(out positionSync))
                positionSync = gameObject.AddComponent<NetworkPositionSync>();
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
            a.OnPositionChanged += OnPivotMoved;
            UpdateVisual();
        }

        public void SetEndPoint(Point b)
        {
            EndPoint = b;
            AddPivot(b);
            b.OnPositionChanged += OnPivotMoved;
            UpdateVisual();
        }

        private void OnPivotMoved(Point moved) => UpdateVisual();

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
                if (child != null)
                    child.gameObject.layer = layer;

            if (StartPoint != null)
                StartPoint.gameObject.layer = layer;
            if (EndPoint != null)
                EndPoint.gameObject.layer = layer;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (ShapeStorage.Contains(this.ShapeId))
                ShapeStorage.Unregister(this);
            if (StartPoint != null) StartPoint.OnPositionChanged -= OnPivotMoved;
            if (EndPoint != null) EndPoint.OnPositionChanged -= OnPivotMoved;
        }

        // ------------------ DRAWER INNER CLASS ------------------ //

        public static class Drawer
        {
            private static Point startPoint;
            private static Point endPoint;
            private static Segment preview;
            private static string pendingStartId;
            private static string pendingEndId;
            private static string pendingSegId;

            private static CreateShapeBatchAction batch;

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
                if (ManipulationManager.Instance.IsDrawing) return;

                ManipulationManager.Instance.IsDrawing = true;

                var datas = new List<ShapeData>();

                Point snap = FindNearbyPoint(pos);
                bool usingSnap = snap != null;

                if (usingSnap)
                {
                    startPoint = snap;
                    pendingStartId = snap.ShapeId;
                }
                else
                {
                    pendingStartId = Guid.NewGuid().ToString();
                    var p1Data = new ShapeData { Id = pendingStartId, Type = "Point", Position = pos, Rotation = Quaternion.identity, Scale = Vector3.one };
                    datas.Add(p1Data);
                }

                pendingEndId = Guid.NewGuid().ToString();
                pendingSegId = Guid.NewGuid().ToString();

                var p2Data = new ShapeData { Id = pendingEndId, Type = "Point", Position = pos, Rotation = Quaternion.identity, Scale = Vector3.one };
                var segData = new ShapeData
                {
                    Id = pendingSegId,
                    Type = "Segment",
                    Position = pos,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new List<string> { pendingStartId, pendingEndId }
                };

                datas.Add(p2Data);
                datas.Add(segData);

                batch = new CreateShapeBatchAction(datas);
                batch.OnShapeSpawned = shape =>
                {
                    if (shape is Point pt)
                    {
                        if (pt.ShapeId == pendingStartId || pt.ShapeId == pendingEndId)
                            OnStartPointReady(pt);
                    }
                    else if (shape is Segment s && s.ShapeId == pendingSegId)
                        OnSegmentReady(s);
                };

                batch.Redo(preview: true); // Tạo bản preview cho tất cả client thấy nhưng chưa ghi vào undo stack

                current = State.Dragging;
            }

            private static void Update()
            {
                if (current != State.Dragging || startPoint == null || endPoint == null) return;
                if (!PerformDrawing.RaycastMouse(out Vector3 pos)) return;

                Point snap = FindNearbyPoint(pos, exclude: startPoint);
                endPoint.MoveTo(snap != null ? snap.transform.position : pos, queue: false);

                if (snap != null && snap != endPoint)
                {
                    endPoint.DestroyShape();
                    preview.SetEndPoint(snap);
                }
            }

            private static void End()
            {
                if (current != State.Dragging) return;
                ManipulationManager.Instance.IsDrawing = false;
                current = State.None;
                PerformDrawing.ResetMode();

                batch = UndoRedoManager.Instance.LastStack() as CreateShapeBatchAction;
                //Debug.LogError($"[Segment] Batch {batch != null}");

                if (endPoint != null && batch != null)
                {
                    var p2Data = batch.shapeDataList.Find(d => d.Id == pendingEndId);
                    //Debug.LogError($"[Segment] p2Data {p2Data != null}");

                    if (p2Data != null)
                    {
                        p2Data.Position = endPoint.transform.position;
                        var a = batch.shapeDataList.Find(d => d.Id == pendingEndId);
                        //Debug.LogError($"[Segment] {p2Data.Position} --- {a.Position}");
                    }
                }

                if (preview != null) preview.SetRaycastIgnore(false);

                if (endPoint != null)
                {
                    var p2Data = batch.shapeDataList.Find(d => d.Id == pendingEndId);
                    if (p2Data != null)
                        p2Data.Position = endPoint.transform.position;
                }

                // ✅ Cập nhật preview thành hành động thật sự
                UndoRedoManager.Instance.ReplaceLast(batch);

                startPoint = null;
                endPoint = null;
                preview = null;
                pendingStartId = null;
                pendingEndId = null;
                pendingSegId = null;
            }

            public static void OnStartPointReady(Point p)
            {
                if (p.ShapeId == pendingStartId)
                    startPoint = p;
                else if (p.ShapeId == pendingEndId)
                    endPoint = p;

                if (preview != null)
                {
                    if (startPoint != null) preview.SetStartPoint(startPoint);
                    if (endPoint != null) preview.SetEndPoint(endPoint);
                }
            }

            public static void OnSegmentReady(Segment s)
            {
                if (s.ShapeId != pendingSegId) return;

                preview = s;
                if (startPoint != null) preview.SetStartPoint(startPoint);
                if (endPoint != null) preview.SetEndPoint(endPoint);

                preview.MarkAsPreview();
                preview.SetRaycastIgnore(true);
            }

            private static Point FindNearbyPoint(Vector3 pos, Point exclude = null)
            {
                foreach (var shape in ShapeStorage.GetAllShapes())
                {
                    if (shape is Point pt && pt != exclude)
                        if (Vector3.Distance(pos, pt.transform.position) < SnapDistance)
                            return pt;
                }
                return null;
            }
        }
    }
}
