using System.Collections.Generic;
using Geometry.Script.Network;
using Manipulator.Data;
using UnityEngine;

namespace Manipulator
{
    public class Segment : Shape, IDrawable2D
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        private ManipulationManager mm;
         
        public Segment(Point start, Point end, Shape parent = null) : base(start.Position, "Segment", parent)
        {
            Start = start;
            End = end;

            GO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            GO.name = Name;

            if (Parent != null)
            {
                GO.transform.SetParent(Parent.GO.transform, false);
                Draw();
            }

            SetupGameObject();
        }

        public Segment(Vector3 only) : this(new Point(only), new Point(only))
        {
            
        }

        private void SetupGameObject()
        {
            Draw2D();
        }

        public override void Drawing()
        {
            ApplyTransform();
        }

        private void DrawPoint()
        {
            Start.Draw();
            End.Draw();
        }

        private void ApplyTransform(bool point = true)
        {
            if (GO == null) return;

            Vector3 offset = Position - Start.Position;
            if (Parent == null)
            {
                Start.Position = Position;
                End.Position += offset;
            }

            Vector3 midPoint = (Start.Position + End.Position) / 2;
            Vector3 direction = End.Position - Start.Position;
            float length = Mathf.Max(direction.magnitude, 0.001f);

            GO.transform.position = midPoint;
            GO.transform.localScale = new Vector3(0.05f, length / 2f, 0.05f);
            GO.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);

            if (point)
            {
                DrawPoint();
            }

        }

        public override void BeginSketch(Vector3 worldPoint)
        {
            mm = ManipulationManager.Instance;

            Point nearestPoint = ShapeStorage.FindNearestPoint(worldPoint);

            Start.SetIgnoreRaycast(true);
            End.SetIgnoreRaycast(true);
            
            if (nearestPoint != null)
            {
                if (nearestPoint != Start)
                {
                    Start.Destroy();
                    Debug.LogError($"Nearest points {nearestPoint.Name}");
                    Start = nearestPoint;
                }
            }
            else
            {
                Start.Destroy();
                Start = new Point(worldPoint);
            }

            mm.SetDrawing(true);
            Start.AttachProcess();
        }

        public override void UpdateSketch(Vector3 worldPoint)
        {
            if (mm == null || !mm.IsDrawing() || Start == null || End == null) return;

            End.Position = worldPoint;
            Draw();
        }

        public override void EndSketch(Vector3 worldPoint)
        {
            if (mm == null || !mm.IsDrawing() || Start == null || End == null) return;

            Point nearestPoint = ShapeStorage.FindNearestPoint(worldPoint);

            if (nearestPoint != null)
            {
                End.Destroy(); // Remove temporary end
                Debug.LogError($"Nearest points {nearestPoint.Name}");
                End = nearestPoint;
            }
            else
            {
                End.Position = worldPoint;
            }

            Start.AttachToShape(this);
            End.AttachToShape(this);

            Start.SetIgnoreRaycast(false);
            End.SetIgnoreRaycast(false);
            
            ApplyTransform();
            CompleteDraw();
            mm.SetDrawing(false);

            End.AttachProcess();
        }


        public override void CompleteDraw()
        {
            UpdateHitbox();

            //GameObject go = new GameObject(Name);
            //go.transform.position -= Position;

            Start.CompleteDraw();
            End.CompleteDraw();

            //GO.transform.parent = go.transform;
            //Start.GO.transform.parent = go.transform;
            //End.GO.transform.parent = go.transform;

            AddPivot(Start);
            AddPivot(End);
            
            base.CompleteDraw();
        }

        protected override void InitializeSettings()
        {
            AppendSettings(new PositionSetting(Position, this));
        }

        public override GameObject[] Components()
        {
            return new[] { GO, Start.GO, End.GO };
        }

        public override void UpdateHitbox()
        {
            if (GO == null) return;

            // Remove existing collider (likely a CapsuleCollider)
            Collider existingCollider = GO.GetComponent<Collider>();
            if (existingCollider != null && !(existingCollider is MeshCollider))
            { 
                Object.DestroyImmediate(existingCollider);
            }

            // Ensure MeshCollider exists
            MeshCollider collider = GO.GetComponent<MeshCollider>();
            if (collider == null)
            {
                collider = GO.AddComponent<MeshCollider>();
            }

            // Refresh the collider with updated mesh
            MeshFilter meshFilter = GO.GetComponent<MeshFilter>();
            if (meshFilter?.sharedMesh != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                mesh.RecalculateBounds();
                collider.sharedMesh = null;
                collider.sharedMesh = mesh;
            }

            collider.convex = false; // Keep non-convex for accuracy
        }


        public void Draw2D()
        {
            // Future implementation (left empty)
        }
        public static class NetStatus
        {
            public static string WhoAmI()
            {
                if (!Unity.Netcode.NetworkManager.Singleton) return "NO_NET";

                var net = Unity.Netcode.NetworkManager.Singleton;

                if (net.IsHost) return "HOST";
                if (net.IsServer) return "SERVER";
                if (net.IsClient) return "CLIENT";

                return "OFFLINE";
            }

            public static ulong MyID()
            {
                return Unity.Netcode.NetworkManager.Singleton?.LocalClientId ?? 9999;
            }
        }


        public override void MovePivots(Point movedPoint)
        {
            string who = NetStatus.WhoAmI();
            ulong clientId = NetStatus.MyID();

            Debug.Log($"[{who} | ClientID: {clientId}] [MovePivots] on Segment '{Name}' due to Point '{movedPoint.Name}' (ID: {movedPoint.id})");
            Debug.Log($"[Before] Start: {Start.Position}, End: {End.Position}, Segment.Position: {Position}");

            if (movedPoint.id == Start.id)
            {
                Debug.Log($"[{who}] ➤ Moving START point.");
                Start.Position = movedPoint.Position;
                Start.GO.transform.position = movedPoint.GO.transform.position;
                Position = Start.Position;
            }
            else if (movedPoint.id == End.id)
            {
                Debug.Log($"[{who}] ➤ Moving END point.");
                End.Position = movedPoint.Position;
                End.GO.transform.position = movedPoint.GO.transform.position;
            }

            Debug.Log($"[After] Start: {Start.Position}, End: {End.Position}, Segment.Position: {Position}");
            DrawPoint();
        }


        
        public void ReloadToConstraint(Point movedPoint)
        {
            MovePivots(movedPoint);
            
            if (GetSNS() != null)
            {
                // GetSNS().MovePivots(movedPoint);
                // ????? chưa có kéo dc cái end thì phải ?????  nó cũng chưa kéo dc start, nó chỉ đang kéo cả cái shape thui
                // kéo point đang chưa có trigger
                
                
                
            }
            
            foreach (RatioCalculator r in GetDependencies().Values)
            {
                r.RecalculatePosition();
            }
            
            
            ApplyTransform(false);
        }

        public override void OnPointMoved(Point movedPoint)
        {
            //Debug.Log($"{Name} updated because {movedPoint.Name} moved.");
            ReloadToConstraint(movedPoint);
            
        }

        public ShapeData Serialize()
        {
            return null;
        }

        public void Deserialize(ShapeData data)
        {
        }
    }
}
