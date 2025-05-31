// Refactored RectangleDrawer with Mesh Display

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class RectangleDrawer : IPrebuiltDrawer
    {
        private Point a, b, c, d;
        private Segment ab, bc, cd, da;
        private string idA, idB, idC, idD;
        private string idAB, idBC, idCD, idDA;
        private ShapeMeshDisplay meshDisplay;

        public void Begin(Vector3 startPos)
        {
            idA = Guid.NewGuid().ToString();
            idB = Guid.NewGuid().ToString();
            idC = Guid.NewGuid().ToString();
            idD = Guid.NewGuid().ToString();
            idAB = Guid.NewGuid().ToString();
            idBC = Guid.NewGuid().ToString();
            idCD = Guid.NewGuid().ToString();
            idDA = Guid.NewGuid().ToString();

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
                new()
                {
                    Id = idD, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one
                },
                new() { Id = idAB, Type = "Segment", ConnectedPoints = new List<string> { idA, idB } },
                new() { Id = idBC, Type = "Segment", ConnectedPoints = new List<string> { idB, idC } },
                new() { Id = idCD, Type = "Segment", ConnectedPoints = new List<string> { idC, idD } },
                new() { Id = idDA, Type = "Segment", ConnectedPoints = new List<string> { idD, idA } }
            };

            var batch = new CreateShapeBatchAction(datas);
            batch.OnShapeSpawned = shape =>
            {
                if (shape is Point pt)
                {
                    if (pt.ShapeId == idA) a = pt;
                    if (pt.ShapeId == idB) b = pt;
                    if (pt.ShapeId == idC) c = pt;
                    if (pt.ShapeId == idD) d = pt;
                }
                else if (shape is Segment s)
                {
                    if (s.ShapeId == idAB) ab = s;
                    if (s.ShapeId == idBC) bc = s;
                    if (s.ShapeId == idCD) cd = s;
                    if (s.ShapeId == idDA) da = s;
                }

                TryConnect();
                TryConnectMesh();
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }

        public void Working(Vector3 currentPos)
        {
            if (a == null || b == null || c == null || d == null) return;
            var snappedPos = currentPos;
            snappedPos.y = 0f;
            var ab = snappedPos - a.transform.position;
            var dir = ab.normalized;
            var width = ab.magnitude;
            var right = Vector3.Cross(Vector3.up, dir);
            var height = width * 0.6f;
            var bPos = a.transform.position + dir * width;
            var cPos = bPos + right * height;
            var dPos = a.transform.position + right * height;
            b.MoveTo(bPos, queue: false);
            c.MoveTo(cPos, queue: false);
            d.MoveTo(dPos, queue: false);
        }


        public void End(Vector3 finalPos)
        {
            foreach (var pt in new[] { a, b, c, d }) pt.SetRaycastIgnore(false);
            foreach (var seg in new[] { ab, bc, cd, da }) seg.SetRaycastIgnore(false);
        }

        public void Cancel()
        {
            foreach (var pt in new[] { a, b, c, d }) pt?.DestroyShape();
            foreach (var seg in new[] { ab, bc, cd, da }) seg?.DestroyShape();
        }

        private void TryConnect()
        {
            if (a != null && b != null && c != null && d != null &&
                ab != null && bc != null && cd != null && da != null)
            {
                foreach (var pt in new[] { a, b, c, d }) pt.SetRaycastIgnore(true);
                foreach (var seg in new[] { ab, bc, cd, da })
                {
                    seg.MarkAsPreview();
                    seg.SetRaycastIgnore(true);
                }

                ab.SetStartPoint(a);
                ab.SetEndPoint(b);
                bc.SetStartPoint(b);
                bc.SetEndPoint(c);
                cd.SetStartPoint(c);
                cd.SetEndPoint(d);
                da.SetStartPoint(d);
                da.SetEndPoint(a);

                if (meshDisplay == null)
                {
                    meshDisplay = a.gameObject.AddComponent<ShapeMeshDisplay>();
                    meshDisplay.Initialize(new List<Point> { a, b, c, d });
                }
            }
        }
        
        private GameObject meshHolder;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        public void TryConnectMesh()
        {
            if (a == null || b == null || c == null || d == null || ab == null) return;

            if (meshHolder == null)
            {
                meshHolder = new GameObject("RectangleMesh");
                meshHolder.transform.SetParent(ab.transform, false);
                meshFilter = meshHolder.AddComponent<MeshFilter>();
                meshRenderer = meshHolder.AddComponent<MeshRenderer>();
                meshCollider = meshHolder.AddComponent<MeshCollider>();
                meshRenderer.sharedMaterial = ab.GetComponent<MeshRenderer>()?.sharedMaterial;
            }

            a.OnPositionChanged -= UpdateMesh;
            b.OnPositionChanged -= UpdateMesh;
            c.OnPositionChanged -= UpdateMesh;
            d.OnPositionChanged -= UpdateMesh;
            a.OnPositionChanged += UpdateMesh;
            b.OnPositionChanged += UpdateMesh;
            c.OnPositionChanged += UpdateMesh;
            d.OnPositionChanged += UpdateMesh;

            UpdateMesh();
        }

        private void UpdateMesh(Point pt = null)
        {
            if (a == null || b == null || c == null || d == null || meshFilter == null) return;
            var verts = new Vector3[] { a.transform.position, b.transform.position, c.transform.position, d.transform.position };
            var mesh = new Mesh();
            mesh.vertices = verts;
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
    }
}