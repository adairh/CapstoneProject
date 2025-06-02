using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Manipulator
{
    public class Polygon : Shape, ShapeMesh
    {

        private MeshCollider meshCollider;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private GameObject visual;
        public List<Point> Points { get; } = new();

        protected override void Awake()
        {
            base.Awake();

            visual = new GameObject("PolygonMesh");
            visual.transform.SetParent(transform, false);

            meshFilter = visual.AddComponent<MeshFilter>();
            meshRenderer = visual.AddComponent<MeshRenderer>();
            meshCollider = visual.AddComponent<MeshCollider>();

            // Shadow settings (adjust as needed)
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // Change to .On if you want casting
            meshRenderer.receiveShadows = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var p in Points)
                if (p != null)
                    p.OnPositionChanged -= OnPivotMoved;
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

            var vertices = Points.Select(p => transform.InverseTransformPoint(p.transform.position)).ToArray();
            var triangles = Triangulate(vertices);

            var mesh = new Mesh
            {
                name = "PolygonMesh",
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.sharedMesh = mesh;

            if (Points.Count >= 3)
            {
                meshCollider.sharedMesh = mesh;
                meshCollider.convex = Points.Count >= 4;

                // Only assign the mesh material ONCE
                if (meshRenderer.sharedMaterial != MeshMat)
                    meshRenderer.sharedMaterial = MeshMat;

                // Only change color via PropertyBlock
                /*var block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", Color.red); // customize as needed
                meshRenderer.SetPropertyBlock(block);*/
            }
            else
            {
                meshCollider.sharedMesh = null;
            }
        }
        
        public void SetMeshHighlightColor(Color color)
        {
            var block = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(block); // preserve other properties
            block.SetColor("_BaseColor", color);
            meshRenderer.SetPropertyBlock(block);
        }


        private int[] Triangulate(Vector3[] vertices)
        {
            List<int> indices = new();
            for (var i = 1; i < vertices.Length - 1; i++)
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
                var shape = ShapeStorage.GetById(id);
                if (shape is Point p)
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
            private const float SnapDistance = 0.2f;
            private static readonly List<Point> points = new();
            private static Segment previewSegment;
            private static Point previewPoint;
            private static string pendingPointId;
            private static string pendingPolygonId;

            // Main entry to call per-frame
            public static void UpdatePolygonInput()
            {
                if (Input.GetMouseButtonDown(0))
                    Click();
                else
                    UpdatePreview();
            }

            private static void Click()
            {
                if (!PerformDrawing.RaycastMouse(out var pos)) return;

                var snap = FindNearbyPoint(pos);
                var closing = snap != null && points.Count >= 3 && snap == points[0];
                if (closing)
                {
                    CompletePolygon();
                    return;
                }

                // Create new point (prefab uses Inspector material)
                var newPointId = Guid.NewGuid().ToString();
                var pointData = new ShapeData
                {
                    Id = newPointId,
                    Type = "Point",
                    Position = pos,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new List<string>(),
                    Settings = new Dictionary<string, string>()
                };

                pendingPointId = newPointId;
                var pointBatch = new CreateShapeBatchAction(new List<ShapeData> { pointData });

                pointBatch.OnShapeSpawned = shape =>
                {
                    if (shape is Point pt && pt.ShapeId == pendingPointId)
                    {
                        // Draw a segment from last point to this point (if not first)
                        if (points.Count > 0)
                        {
                            var segId = Guid.NewGuid().ToString();
                            var segData = new ShapeData
                            {
                                Id = segId,
                                Type = "Segment",
                                Position = Vector3.zero,
                                Rotation = Quaternion.identity,
                                Scale = Vector3.one,
                                ConnectedPoints = new List<string> { points.Last().ShapeId, pt.ShapeId },
                                Settings = new Dictionary<string, string>()
                            };
                            var segBatch = new CreateShapeBatchAction(new List<ShapeData> { segData });
                            UndoRedoNetworkBridge.Instance.DoAndBroadcast(segBatch);
                        }

                        points.Add(pt);

                        // Create/update preview objects
                        if (previewPoint == null)
                        {
                            previewPoint = ShapeFactory.CreateShape("Point", pos) as Point;
                            ShapeStorage.Unregister(previewPoint); // Hide from global selection
                            previewPoint.SetRaycastIgnore(true);   // Ignore raycasts
                        }
                        if (previewSegment == null)
                        {
                            previewSegment = ShapeFactory.CreateShape("Segment", pos) as Segment;
                            previewSegment.MarkAsPreview();
                            previewSegment.SetRaycastIgnore(true);
                        }
                        previewSegment.SetStartPoint(pt);
                        previewSegment.SetEndPoint(previewPoint);
                    }
                };

                UndoRedoNetworkBridge.Instance.DoAndBroadcast(pointBatch);
            }

            private static void UpdatePreview()
            {
                if (previewPoint == null || previewSegment == null || points.Count == 0) return;
                if (!PerformDrawing.RaycastMouse(out var pos)) return;

                previewPoint.MoveTo(pos, queue: false);
                previewSegment.SetEndPoint(previewPoint);
            }

            private static void CompletePolygon()
            {
                if (points.Count < 3) return;

                // Final segment from last point to first
                var lastSegId = Guid.NewGuid().ToString();
                var finalSegData = new ShapeData
                {
                    Id = lastSegId,
                    Type = "Segment",
                    Position = Vector3.zero,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new List<string> { points.Last().ShapeId, points[0].ShapeId },
                    Settings = new Dictionary<string, string>()
                };
                var segBatch = new CreateShapeBatchAction(new List<ShapeData> { finalSegData });
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(segBatch);

                // Create the polygon shape
                pendingPolygonId = Guid.NewGuid().ToString();
                var polyData = new ShapeData
                {
                    Id = pendingPolygonId,
                    Type = "Polygon",
                    Position = Vector3.zero,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = points.Select(p => p.ShapeId).ToList(),
                    Settings = new Dictionary<string, string>()
                };
                var polyAction = new CreateShapeBatchAction(new List<ShapeData> { polyData });
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(polyAction);

                // Cleanup preview objects
                points.Clear();

                if (previewPoint != null)
                {
                    ShapeStorage.Unregister(previewPoint);
                    UnityEngine.Object.Destroy(previewPoint.gameObject);
                    previewPoint = null;
                }
                if (previewSegment != null)
                {
                    ShapeStorage.Unregister(previewSegment);
                    UnityEngine.Object.Destroy(previewSegment.gameObject);
                    previewSegment = null;
                }

                PerformDrawing.ResetMode();
            }

            private static Point FindNearbyPoint(Vector3 pos)
            {
                foreach (var shape in ShapeStorage.GetAllShapes())
                    if (shape is Point p && Vector3.Distance(p.transform.position, pos) < SnapDistance)
                        return p;
                return null;
            }
        }
    }
}
