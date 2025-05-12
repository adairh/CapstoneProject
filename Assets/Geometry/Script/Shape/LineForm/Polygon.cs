using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Manipulator;

namespace Manipulator
{
    public class Polygon : Shape
    {
        public List<Point> Points { get; private set; } = new();

        private GameObject visual;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        protected override void Awake()
        {
            base.Awake();

            visual = new GameObject("PolygonMesh");
            visual.transform.SetParent(transform, false);

            meshFilter = visual.AddComponent<MeshFilter>();
            meshRenderer = visual.AddComponent<MeshRenderer>();
            meshCollider = visual.AddComponent<MeshCollider>();

            meshRenderer.material = DefaultMat;
            meshCollider.convex = true;
        }

        public void AddPoint(Point p)
        {
            if (!Points.Contains(p))
            {
                Points.Add(p);
                AddPivot(p);
                p.OnPositionChanged += OnPivotMoved;
            }
        }

        public void CompletePolygon()
        {
            if (Points.Count < 3) return;
            GenerateMesh();
        }

        private void GenerateMesh()
        {
            if (Points.Count < 3) return;

            Vector3[] vertices = Points.Select(p => transform.InverseTransformPoint(p.transform.position)).ToArray();
            int[] triangles = Triangulate(vertices);

            var mesh = new Mesh
            {
                name = "PolygonMesh",
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds(); 

            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh; 
        }

        private int[] Triangulate(Vector3[] vertices)
        {
            List<int> indices = new();
            for (int i = 1; i < vertices.Length - 1; i++)
            {
                indices.Add(0);
                indices.Add(i);
                indices.Add(i + 1);
            }
            return indices.ToArray();
        }

        private void OnPivotMoved(Point pt)
        {
            GenerateMesh();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var p in Points)
            {
                if (p != null)
                    p.OnPositionChanged -= OnPivotMoved;
            }
        }

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "Polygon";
            data.ConnectedPoints = Points.Select(p => p.ShapeId).ToList();
            return data;
        }

        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);
            Points.Clear();

            foreach (var id in data.ConnectedPoints)
            {
                Shape shape = ShapeStorage.GetById(id);   
                if ( shape is Point p)
                    AddPoint(p);
            }
            CompletePolygon();
        }

        public override IEnumerable<Shape> GetDependentShapesForDelete()
        {
            yield return this;
            foreach (var p in Points)
                if (p != null && p.IsOnlyConnectedTo(this))
                    yield return p;
        }
 
        public static class Drawer
        {
            private static List<Point> points = new();
            private static Segment previewSegment;
            private static Point previewPoint;
            private static string pendingPointId;
            private static string pendingPolygonId;
            private static CreateShapeBatchAction batch;
            private const float SnapDistance = 0.2f;

            public static void UpdatePolygonInput()
            {
                if (Input.GetMouseButtonDown(0)) Click();
                else UpdatePreview();
            }

            private static void Click()
            {
                if (!PerformDrawing.RaycastMouse(out Vector3 pos)) return;

                Point snap = FindNearbyPoint(pos);
                bool closing = snap != null && points.Count >= 3 && snap == points[0];
                if (closing)
                {
                    CompletePolygon();
                    return;
                }

                // Spawn fixed point
                string newPointId = Guid.NewGuid().ToString();
                var pointData = new ShapeData
                {
                    Id = newPointId,
                    Type = "Point",
                    Position = pos,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new(),
                    Settings = new()
                };

                pendingPointId = newPointId;

                batch = new CreateShapeBatchAction(new List<ShapeData> { pointData });
                batch.OnShapeSpawned = shape =>
                {
                    if (shape is Point pt && pt.ShapeId == pendingPointId)
                    {
                        points.Add(pt);

                        // Chỉ tạo preview sau khi có ít nhất 2 điểm
                        if (points.Count > 1)
                        {
                            if (previewPoint == null)
                            {
                                previewPoint = ShapeFactory.CreateShape("Point", pos) as Point;
                                previewPoint.SetRaycastIgnore(true);
                            }

                            if (previewSegment == null)
                            {
                                previewSegment = ShapeFactory.CreateShape("Segment", pos) as Segment;
                                previewSegment.MarkAsPreview();
                                previewSegment.SetStartPoint(pt);      // ✅ Dùng pt trực tiếp
                                previewSegment.SetEndPoint(previewPoint);
                            }
                            else
                            {
                                // Reset start point cho đoạn mới
                                previewSegment.SetStartPoint(pt);
                            }
                        }
                    }
                };

                UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
            }

            private static void UpdatePreview()
            {
                if (previewPoint == null || previewSegment == null || points.Count == 0) return;
                if (!PerformDrawing.RaycastMouse(out Vector3 pos)) return;

                previewPoint.MoveTo(pos, queue: false);
                previewSegment.SetEndPoint(previewPoint);
            }

            private static void CompletePolygon()
            {
                if (points.Count < 3) return;

                pendingPolygonId = Guid.NewGuid().ToString();
                var polyData = new ShapeData
                {
                    Id = pendingPolygonId,
                    Type = "Polygon",
                    Position = Vector3.zero,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = points.Select(p => p.ShapeId).ToList(),
                    Settings = new()
                };

                var action = new CreateShapeBatchAction(new List<ShapeData> { polyData });
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(action);

                // Cleanup
                points.Clear();

                previewPoint?.DestroyShape();
                previewSegment?.DestroyShape();
                previewPoint = null;
                previewSegment = null;

                PerformDrawing.ResetMode();
            }

            private static Point FindNearbyPoint(Vector3 pos)
            {
                foreach (var shape in ShapeStorage.GetAllShapes())
                {
                    if (shape is Point p && Vector3.Distance(p.transform.position, pos) < SnapDistance)
                        return p;
                }
                return null;
            }
        }
    }
}
