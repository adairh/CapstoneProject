// Refactored EquilateralTriangleDrawer with Mesh Display

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class EquilateralTriangleDrawer : IPrebuiltDrawer
    {
        private Point a, b, c;
        private Segment ab, bc, ca;
        private float baseLength;
        private string idA, idB, idC, idAB, idBC, idCA;
        private ShapeMeshDisplay meshDisplay;
        
        
        private GameObject meshHolder;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        public void Begin(Vector3 startPos)
        {
            idA = Guid.NewGuid().ToString();
            idB = Guid.NewGuid().ToString();
            idC = Guid.NewGuid().ToString();
            idAB = Guid.NewGuid().ToString();
            idBC = Guid.NewGuid().ToString();
            idCA = Guid.NewGuid().ToString();

            var datas = new List<ShapeData>
            {
                new()
                {
                    Id = idA, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one
                },
                new()
                {
                    Id = idB, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one
                },
                new()
                {
                    Id = idC, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one
                },
                new() { Id = idAB, Type = "Segment", ConnectedPoints = new List<string> { idA, idB } },
                new() { Id = idBC, Type = "Segment", ConnectedPoints = new List<string> { idB, idC } },
                new() { Id = idCA, Type = "Segment", ConnectedPoints = new List<string> { idC, idA } }
            };

            var batch = new CreateShapeBatchAction(datas);
            batch.OnShapeSpawned = shape =>
            {
                if (shape is Point pt)
                {
                    if (pt.ShapeId == idA) a = pt;
                    if (pt.ShapeId == idB) b = pt;
                    if (pt.ShapeId == idC) c = pt;
                }

                if (shape is Segment s)
                {
                    if (s.ShapeId == idAB) ab = s;
                    if (s.ShapeId == idBC) bc = s;
                    if (s.ShapeId == idCA) ca = s;
                }

                TryConnectSegments();
                TryConnect();
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }

        public void Working(Vector3 currentPos)
        {
            if (a == null || b == null || c == null) return;
            var snappedPos = currentPos;
            snappedPos.y = 0f;
            b.MoveTo(snappedPos, queue: false);
            var dir = (b.transform.position - a.transform.position).normalized;
            var baseLength = Vector3.Distance(a.transform.position, b.transform.position);
            var right = Vector3.Cross(Vector3.up, dir); // Plane is XZ
            var height = Mathf.Sqrt(3f) / 2f * baseLength;
            var midpoint = (a.transform.position + b.transform.position) / 2f;
            var cPos = midpoint + right * height;
            c.MoveTo(cPos, queue: false);
        }


        public void End(Vector3 finalPos)
        {
            a.SetRaycastIgnore(false);
            b.SetRaycastIgnore(false);
            c.SetRaycastIgnore(false);
            ab.SetRaycastIgnore(false);
            bc.SetRaycastIgnore(false);
            ca.SetRaycastIgnore(false);
        }

        public void Cancel()
        {
            a?.DestroyShape();
            b?.DestroyShape();
            c?.DestroyShape();
            ab?.DestroyShape();
            bc?.DestroyShape();
            ca?.DestroyShape();
        }

        private void TryConnectSegments()
        {
            if (a != null && b != null && c != null && ab != null && bc != null && ca != null)
            {
                a.SetRaycastIgnore(true);
                b.SetRaycastIgnore(true);
                c.SetRaycastIgnore(true);
                ab.SetRaycastIgnore(true);
                bc.SetRaycastIgnore(true);
                ca.SetRaycastIgnore(true);
                ab.MarkAsPreview();
                bc.MarkAsPreview();
                ca.MarkAsPreview();

                ab.SetStartPoint(a);
                ab.SetEndPoint(b);
                bc.SetStartPoint(b);
                bc.SetEndPoint(c);
                ca.SetStartPoint(c);
                ca.SetEndPoint(a);

                if (meshDisplay == null)
                {
                    meshDisplay = a.gameObject.AddComponent<ShapeMeshDisplay>();
                    meshDisplay.Initialize(new List<Point> { a, b, c });
                }
            }
        }

        public void TryConnect()
        {
            if (a == null || b == null || c == null || ab == null) return;

            if (meshHolder == null)
            {
                meshHolder = new GameObject("TriangleMesh");
                meshHolder.transform.SetParent(ab.transform, false);
                meshFilter = meshHolder.AddComponent<MeshFilter>();
                meshRenderer = meshHolder.AddComponent<MeshRenderer>();
                meshCollider = meshHolder.AddComponent<MeshCollider>();
                meshRenderer.sharedMaterial = ab.GetComponent<MeshRenderer>()?.sharedMaterial;
            }

            a.OnPositionChanged -= UpdateMesh;
            b.OnPositionChanged -= UpdateMesh;
            c.OnPositionChanged -= UpdateMesh;
            a.OnPositionChanged += UpdateMesh;
            b.OnPositionChanged += UpdateMesh;
            c.OnPositionChanged += UpdateMesh;

            UpdateMesh();
        }

        private void UpdateMesh(Point pt = null)
        {
            if (a == null || b == null || c == null || meshFilter == null) return;
            var verts = new Vector3[] { a.transform.position, b.transform.position, c.transform.position };
            var mesh = new Mesh();
            mesh.vertices = verts;
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
    }
}