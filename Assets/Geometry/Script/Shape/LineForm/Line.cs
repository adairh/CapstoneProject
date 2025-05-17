// Refactored Line.cs — ADD DEBUG LOGGING FOR DRAWING FLOW with SNAP POINTS and UNDO-SAFE SPAWN + RECONNECT SYNC FIX + SAFE DESTROY HANDLING

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class Line : Shape
    {
        [SerializeField] private string IDS, IDE;
        private bool isPreview;
        private NetworkPositionSync positionSync;

        private GameObject visual;
        public Point StartPoint { get; private set; }
        public Point EndPoint { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            var meshFilter = gameObject.AddComponent<MeshFilter>();
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            var meshCollider = gameObject.AddComponent<MeshCollider>();

// Create a cylinder mesh with height = 1
            var mesh = MeshGenerator.CreateCylinder(1f, 0.05f);
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = true;

            meshRenderer.material = DefaultMat;


            if (!TryGetComponent(out positionSync))
                positionSync = gameObject.AddComponent<NetworkPositionSync>();
        }

        private void Update()
        {
            if (!isPreview || StartPoint == null || visual == null) return;

            var a = StartPoint.transform.position;
            var b = EndPoint != null ? EndPoint.transform.position : a;
            var mid = (a + b) / 2;
            var dir = b - a;
            var length = dir.magnitude;

            visual.transform.position = mid;
            visual.transform.rotation = Quaternion.LookRotation(dir);
            visual.transform.Rotate(90, 0, 0);
            visual.transform.localScale = new Vector3(0.05f, length / 2f, 0.05f);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (ShapeStorage.Contains(ShapeId))
                ShapeStorage.Unregister(this);
            if (StartPoint != null) StartPoint.OnPositionChanged -= OnPivotMoved;
            if (EndPoint != null) EndPoint.OnPositionChanged -= OnPivotMoved;
        }

        public override void UpdateHitbox()
        {
            if (TryGetComponent(out MeshCollider col))
            {
                col.sharedMesh = MeshGenerator.CreateCylinder(1f, 0.05f);
                col.convex = true;
            }
        }

        public override void MoveTo(Vector3 newPosition, bool silent = false, bool queue = true)
        {
            var delta = newPosition - transform.position;

            if (StartPoint == null || EndPoint == null) return;

            var moves = new List<(string, Vector3, Vector3)>
            {
                (StartPoint.ShapeId, StartPoint.transform.position, StartPoint.transform.position + delta),
                (EndPoint.ShapeId, EndPoint.transform.position, EndPoint.transform.position + delta)
            };

            if (!silent && !isInternalMove)
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(new MultiMoveShapeAction(moves), queue);

            // Di chuyển các point luôn
            StartPoint.isInternalMove = true;
            EndPoint.isInternalMove = true;

            UndoRedoManager.SuppressRecording = true;

            StartPoint.MoveTo(StartPoint.transform.position + delta, silent);
            EndPoint.MoveTo(EndPoint.transform.position + delta, silent);

            UndoRedoManager.SuppressRecording = false;


            StartPoint.isInternalMove = false;
            EndPoint.isInternalMove = false;
        }

        public override IEnumerable<Shape> GetDependentShapesForDelete()
        {
            yield return this;
            if (StartPoint != null && StartPoint.IsOnlyConnectedTo(this)) yield return StartPoint;
            if (EndPoint != null && EndPoint.IsOnlyConnectedTo(this)) yield return EndPoint;
        }

        public void MarkAsPreview()
        {
            isPreview = true;
        }

        public void SetStartPoint(Point a)
        {
            StartPoint = a;
            AddPivot(a);
            a.OnPositionChanged += OnPivotMoved; // 👈 thêm dòng này
            UpdateVisual();
        }

        public void SetEndPoint(Point b)
        {
            EndPoint = b;
            AddPivot(b);
            b.OnPositionChanged += OnPivotMoved; // 👈 thêm dòng này
            UpdateVisual();
        }

        private void OnPivotMoved(Point moved)
        {
            UpdateVisual(); // 👈 tự cập nhật lại visual khi point di chuyển
        }

        private void UpdateVisual()
        {
            if (StartPoint == null || EndPoint == null) return;

            IDS = StartPoint.ShapeId;
            IDE = EndPoint.ShapeId;

            var a = StartPoint.transform.position;
            var b = EndPoint.transform.position;

            var dir = (b - a).normalized;

            // Kéo dài 100 đơn vị mỗi phía (vô cực mô phỏng)
            var farA = a - dir * 50f;
            var farB = b + dir * 50f;

            var mid = (farA + farB) / 2;
            var length = Vector3.Distance(farA, farB);

            transform.position = mid;
            transform.rotation = Quaternion.LookRotation(farB - farA);
            transform.Rotate(90, 0, 0); // Align cylinder Y-axis to world Z-axis (optional)
            transform.localScale = new Vector3(1f, length, 1f); // y là height do mesh tạo sẵn là 1


            UpdateHitbox();
        }

        protected override void OnPivotChanged(Point pt)
        {
            base.OnPivotChanged(pt);
            UpdateVisual();
        }

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "Line";
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
            var layer = ignore ? 2 : 0;
            gameObject.layer = layer;

            foreach (Transform child in transform)
                if (child != null)
                    child.gameObject.layer = layer;

            if (StartPoint != null && StartPoint.gameObject != null)
                StartPoint.gameObject.layer = layer;

            if (EndPoint != null && EndPoint.gameObject != null)
                EndPoint.gameObject.layer = layer;
        }

        public static class Drawer
        {
            private const float SnapDistance = 0.3f;
            private static Point startPoint;
            private static Point endPoint;
            private static Line preview;
            private static string pendingStartId;
            private static string pendingEndId;
            private static string pendingSegId;

            private static CreateShapeBatchAction batch;
            private static State current = State.None;

            public static void UpdateLineInput()
            {
                if (Input.GetMouseButtonDown(0)) Start();
                else if (Input.GetMouseButton(0)) Update();
                else if (Input.GetMouseButtonUp(0)) End();
            }

            private static void Start()
            {
                if (!PerformDrawing.RaycastMouse(out var pos)) return;
                var mm = ManipulationManager.Instance;
                if (mm.IsDrawing) return;
                mm.IsDrawing = true;

                List<ShapeData> datas = new();

                var snap = FindNearbyPoint(pos);
                var usingSnap = snap != null;

                if (usingSnap)
                {
                    startPoint = snap;
                    pendingStartId = snap.ShapeId;
                }
                else
                {
                    pendingStartId = Guid.NewGuid().ToString();
                    var p1Data = new ShapeData
                    {
                        Id = pendingStartId, Type = "Point", Position = pos, Rotation = Quaternion.identity,
                        Scale = Vector3.one, ConnectedPoints = new List<string>(),
                        Settings = new Dictionary<string, string>()
                    };
                    datas.Add(p1Data);
                }

                pendingEndId = Guid.NewGuid().ToString();
                pendingSegId = Guid.NewGuid().ToString();

                var p2Data = new ShapeData
                {
                    Id = pendingEndId, Type = "Point", Position = pos, Rotation = Quaternion.identity,
                    Scale = Vector3.one, ConnectedPoints = new List<string>(),
                    Settings = new Dictionary<string, string>()
                };
                var segData = new ShapeData
                {
                    Id = pendingSegId, Type = "Line", Position = pos, Rotation = Quaternion.identity,
                    Scale = Vector3.one, ConnectedPoints = new List<string> { pendingStartId, pendingEndId },
                    Settings = new Dictionary<string, string>()
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
                    else if (shape is Line s && s.ShapeId == pendingSegId)
                    {
                        OnLineReady(s);
                    }
                };

                UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);

                current = State.Dragging;
            }

            private static void Update()
            {
                if (current != State.Dragging || startPoint == null || endPoint == null) return;
                if (!PerformDrawing.RaycastMouse(out var pos)) return;

                var snap = FindNearbyPoint(pos, startPoint);
                endPoint.MoveTo(snap != null ? snap.transform.position : pos, queue: false);

                // if (snap != null && snap != endPoint)
                // {
                //     endPoint.DestroyShape();
                //     preview.SetEndPoint(snap);
                // }
            }

            private static void End()
            {
                if (current != State.Dragging) return;
                ManipulationManager.Instance.IsDrawing = false;
                current = State.None;
                PerformDrawing.ResetMode();

                var pos = endPoint.transform.position;
                var snap = FindNearbyPoint(pos, startPoint);
                endPoint.MoveTo(snap != null ? snap.transform.position : pos, queue: false);

                batch = UndoRedoManager.Instance.LastStack() as CreateShapeBatchAction;

                if (snap != null && snap != endPoint)
                {
                    endPoint.DestroyShape();
                    preview.SetEndPoint(snap);

                    //Debug.LogError($"[Line - new] {snap.ShapeId}");

                    if (batch != null)
                    {
                        var p2Data = batch.shapeDataList.Find(d => d.Id == pendingEndId);
                        //Debug.LogError($"[Line] p2Data {p2Data != null}");

                        if (p2Data != null)
                            /*Debug.LogError($"[Line - old] {p2Data.Id}");
    
    
                                foreach (var i in batch.shapeDataList)
                                {
                                    Debug.LogError($"[Line - before] {i.Id}");
    
                                }*/
                            if (p2Data.Id != snap.ShapeId)
                            {
                                batch.shapeDataList.Remove(p2Data);
                                batch.shapeDataList.Add(snap.Data);

                                batch.createdShapes.Remove(endPoint);
                                batch.createdShapes.Remove(snap);

                                var seg = batch.shapeDataList.Find(d => d.Id == pendingSegId);

                                /*foreach (var i in seg.ConnectedPoints)
                                {
                                    Debug.LogError($"[LineCon - before] {i}");

                                }*/

                                List<string> conn = new() { pendingStartId, snap.ShapeId };
                                seg.ConnectedPoints = conn;

                                /*foreach (var i in seg.ConnectedPoints)
                                {
                                    Debug.LogError($"[LineCon - after] {i}");

                                }*/
                            }
                        /*foreach (var i in batch.shapeDataList)
                            {
                                Debug.LogError($"[Line - after] {i.Id}");

                            }*/
                    }
                }

                //Debug.LogError($"[Line] Batch {batch != null}");

                if (endPoint != null && batch != null)
                {
                    var p2Data = batch.shapeDataList.Find(d => d.Id == pendingEndId);
                    //Debug.LogError($"[Line] p2Data {p2Data != null}");

                    if (p2Data != null)
                    {
                        p2Data.Position = endPoint.transform.position;
                        var a = batch.shapeDataList.Find(d => d.Id == pendingEndId);
                        //Debug.LogError($"[Line] {p2Data.Position} --- {a.Position}");
                    }
                }

                UndoRedoManager.Instance.ReplaceStack(batch);

                if (preview != null) preview.SetRaycastIgnore(false);
                startPoint = null;
                endPoint = null;
                preview = null;
                pendingStartId = null;
                pendingEndId = null;
                pendingSegId = null;
            }

            public static void OnStartPointReady(Point p)
            {
                //Debug.LogError($"[OnStartPointReady] Called with Point ID = {p.ShapeId}");
                //Debug.LogError($"[OnStartPointReady] Current pendingStartId = {pendingStartId}, pendingEndId = {pendingEndId}");

                if (p.ShapeId == pendingStartId)
                {
                    startPoint = p;
                }
                else if (p.ShapeId == pendingEndId)
                {
                    endPoint = p;
                }
                else
                    //Debug.LogError("[OnStartPointReady] Point ID does not match any pending ID");

                if (preview != null)
                {
                    if (startPoint != null) preview.SetStartPoint(startPoint);
                    if (endPoint != null) preview.SetEndPoint(endPoint);
                }
            }

            public static void OnLineReady(Line s)
            {
                //Debug.LogError($"[OnLineReady] Called with Line ID = {s.ShapeId}");
                //Debug.LogError($"[OnLineReady] Pending ID = {pendingSegId}");

                if (s.ShapeId != pendingSegId)
                    //Debug.LogError("[OnLineReady] Line ID does not match");
                    return;

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
                    if (shape == null || shape.gameObject == null) continue;

                    if (shape is Point pt && pt != exclude)
                        if (Vector3.Distance(pos, pt.transform.position) < SnapDistance)
                            return pt;
                }

                return null;
            }

            private enum State
            {
                None,
                Dragging
            }
        }
    }
}