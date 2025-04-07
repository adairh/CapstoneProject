using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class StraightLine : Shape, IDrawable2D
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        private static Vector3 startPoint;
        private static StraightLine currentStraightLine;
        private static ManipulationManager mm;
        
        public StraightLine(Point start, Point end, Shape parent = null) : base(start.Position, "StraightLine", parent)
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

        public static void Sketch(Vector3 worldPoint, Camera mainCamera)
        {
            mm = ManipulationManager.Instance;

            if (mm.IsDrawing())
            {
                if (mm.ModeStraight == ManipulationManager.Straight.X)
                {
                    worldPoint.y = startPoint.y;
                    worldPoint.z = startPoint.z;
                }
                else if (mm.ModeStraight == ManipulationManager.Straight.Y)
                {
                    worldPoint.x = startPoint.x;
                    worldPoint.z = startPoint.z;
                }
                else if (mm.ModeStraight == ManipulationManager.Straight.Z)
                {
                    worldPoint.x = startPoint.x;
                    worldPoint.y = startPoint.y;
                }
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                if (!mm.IsDrawing())
                {
                    StartSketch(worldPoint);
                }
                else
                {
                    CompleteSketch(worldPoint);
                }
            }

            if (mm.IsDrawing())
            {
                currentStraightLine.End.Position = worldPoint;
                currentStraightLine.Draw();
            }
        }

        private static void StartSketch(Vector3 worldPoint)
        {
            Point nearestPoint = ShapeStorage.FindNearestPoint(worldPoint);

            if (nearestPoint != null)
            {
                startPoint = nearestPoint.Position;
                currentStraightLine = new StraightLine(nearestPoint, new Point(startPoint));
            }
            else
            {
                startPoint = worldPoint;
                Point start = new Point(startPoint);
                currentStraightLine = new StraightLine(start, new Point(startPoint));
            }

            mm.SetDrawing(true);
        }


        private static void CompleteSketch(Vector3 worldPoint)
        {
            Point nearestPoint = ShapeStorage.FindNearestPoint(worldPoint);
            
            if (nearestPoint != null)
            {
                currentStraightLine.End.Destroy(); // Remove temporary end
                Debug.LogError($"Nearest points {nearestPoint.Name}");
                currentStraightLine.End = nearestPoint;
            }
            else
            {
                currentStraightLine.End.Position = worldPoint;
            }

            currentStraightLine.Start.AttachToShape(currentStraightLine);
            currentStraightLine.End.AttachToShape(currentStraightLine);

            currentStraightLine.ApplyTransform(); 
            currentStraightLine.CompleteDraw();
            mm.SetDrawing(false);

            
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

        public void ReloadToConstraint(Point movedPoint)
        {
            if (movedPoint.id == Start.id)
            {
                Start.Position = movedPoint.Position;
                Start.GO.transform.position = movedPoint.GO.transform.position;
                Position = Start.Position;
            }
            else if (movedPoint.id == End.id)
            {
                End.Position = movedPoint.Position;
                End.GO.transform.position = movedPoint.GO.transform.position;
            }

            ApplyTransform(false);
        }

        public override void OnPointMoved(Point movedPoint)
        {
            //Debug.Log($"{Name} updated because {movedPoint.Name} moved.");
            ReloadToConstraint(movedPoint);
        }
    }
}
