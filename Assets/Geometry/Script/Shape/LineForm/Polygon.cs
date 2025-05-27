using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Manipulator
{
    public class Polygon : Shape
    {
        private MeshCollider meshCollider;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Material _polygonMeshMat;

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
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_polygonMeshMat != null)
                Destroy(_polygonMeshMat);
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

                // Ensure our custom double-sided transparent mat is always applied
                if (_polygonMeshMat != null)
                    Destroy(_polygonMeshMat);
                _polygonMeshMat = CreatePolygonMeshMaterial();
                meshRenderer.material = _polygonMeshMat;
            }
            else
            {
                meshCollider.sharedMesh = null;
            }
        }

        private Material CreatePolygonMeshMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard"); // fallback

            var mat = new Material(shader);

            Color color = new Color(0.4f, 0.8f, 1f, 0.08f);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else
                mat.color = color;

            // Transparency
            if (mat.HasProperty("_Surface"))
                mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            if (mat.HasProperty("_ZWrite"))
                mat.SetInt("_ZWrite", 0);
            if (mat.HasProperty("_SrcBlend"))
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend"))
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Double-sided
            if (mat.HasProperty("_CullMode"))
                mat.SetInt("_CullMode", 0);
            if (mat.HasProperty("_Cull"))
                mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            if (mat.HasProperty("_RenderFace"))
                mat.SetInt("_RenderFace", 0);
            mat.EnableKeyword("_DOUBLESIDED_ON");
            mat.doubleSidedGI = true;

            return mat;
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

        // ------------------------ DRAWER LOGIC ------------------------

        public static class Drawer
        {
            private const float SnapDistance = 0.2f;
            private static readonly List<Point> points = new();
            private static Segment previewSegment;
            private static Point previewPoint;
            private static string pendingPointId;
            private static string pendingPolygonId;

            public static void UpdatePolygonInput()
            {
                if (Input.GetMouseButtonDown(0)) Click();
                else UpdatePreview();
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

                // Create real point
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
                        // Spawn segment from last to new point
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

                        // Create/update preview
                        if (previewPoint == null)
                        {
                            previewPoint = ShapeFactory.CreateShape("Point", pos) as Point;
                            ShapeStorage.Unregister(previewPoint);
                            previewPoint.SetRaycastIgnore(true);
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

                // Create last segment from last → first point
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

                // Create the polygon
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

                // Before cleanup: turn previewPoint into a real point if necessary
                if (previewPoint != null)
                {
                    previewPoint.SetRaycastIgnore(false);
                    ShapeStorage.Register(previewPoint);
                    points.Add(previewPoint);
                }

                // Cleanup
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

                previewPoint?.DestroyShape();
                previewSegment?.DestroyShape();
                previewPoint = null;
                previewSegment = null;

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
