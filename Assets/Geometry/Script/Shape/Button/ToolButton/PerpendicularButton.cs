using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;  // If using the new Input System for Esc key (optional)

namespace Manipulator
{
public class PerpendicularButton : BaseButton
    {
        private GameObject ghostLine;
        private LineRenderer ghostLineRenderer;
        private Segment baseSegment;

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            
            bool anySegment = false;
            foreach (var shape in ShapeStorage.GetAllShapes())
                if (shape is Segment) { anySegment = true; break; }

            if (!anySegment)
            {
                UIHint.ShowTemp("No segment available in scene!", 2f);
                return; // Cancel tool activation
            }
            
            StartCoroutine(PerpendicularLineRoutine());
        }

        private IEnumerator PerpendicularLineRoutine()
        {
            ManipulationManager.Instance.IsDrawing = true;
            UIHint.Show("Select a segment to make a perpendicular line");
            baseSegment = null;
            yield return ShapePicker.WaitForSegment();
            baseSegment = ShapePicker.LastPicked as Segment;
            UIHint.Hide();

            if (baseSegment == null)
            {
                CleanupPreview();
                ManipulationManager.Instance.IsDrawing = false;
                yield break;
            }

            // Calculate perpendicular direction to base segment
            Vector3 baseDir = (baseSegment.EndPoint.transform.position - baseSegment.StartPoint.transform.position).normalized;
            Vector3 perpDir = Vector3.Cross(baseDir, Vector3.up).normalized;
            if (perpDir == Vector3.zero) perpDir = Vector3.Cross(baseDir, Vector3.forward).normalized;
            SpawnGhostLine(perpDir);

            UIHint.Show("Select a point through which the perpendicular line will pass (or click empty space)");
            Point chosenPoint = null;
            Vector3 targetPos = Vector3.zero;
            bool selectionMade = false;

            while (!selectionMade)
            {
                UpdateGhostLine(perpDir);

                if (Input.GetMouseButtonDown(1) || Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    UIHint.Hide();
                    CleanupPreview();
                    ManipulationManager.Instance.IsDrawing = false;
                    yield break;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (PerformDrawing.RaycastMouse(out Vector3 hitPos, out Shape hitShape))
                    {
                        if (hitShape is Point pointShape)
                        {
                            chosenPoint = pointShape;
                            targetPos = pointShape.transform.position;
                        }
                        else
                        {
                            chosenPoint = null;
                            targetPos = hitPos;
                        }
                        selectionMade = true;
                        ManipulationManager.Instance.TrackingPoint = targetPos;
                    }
                }

                yield return null;
            }

            UIHint.Hide();
            CleanupPreview();

            GeometryTool.CreatePerpendicularThrough(
                baseSegment.StartPoint,
                baseSegment.EndPoint,
                chosenPoint,
                3f
            );

            Segment newSegment = ShapeStorage.GetMostRecentSegment();
            if (newSegment != null)
            {
                var data = new PerpendicularConstraintData
                {
                    ConstraintId = System.Guid.NewGuid().ToString(),
                    Segment1Id = baseSegment.ShapeId,
                    Segment2Id = newSegment.ShapeId,
                    Type = "Perpendicular"
                };
                ConstraintFactory.CreateConstraintNetworked(data);
            }

            ManipulationManager.Instance.IsDrawing = false;
            
            PerformDrawing.ResetMode();
        }

        private void SpawnGhostLine(Vector3 direction)
        {
            ghostLine = new GameObject("GhostLinePreview");
            ghostLineRenderer = ghostLine.AddComponent<LineRenderer>();
            ghostLineRenderer.positionCount = 2;
            ghostLineRenderer.startWidth = 0.05f;
            ghostLineRenderer.endWidth = 0.05f;
            ghostLineRenderer.material = MaterialLibrary.Get(MaterialType.Blue);
            ghostLineRenderer.startColor = new Color(1, 1, 1, 0.5f);
            ghostLineRenderer.endColor = new Color(1, 1, 1, 0.5f);
            // Initialize with base segment orientation as reference
            ghostLineRenderer.SetPosition(0, baseSegment.StartPoint.transform.position);
            ghostLineRenderer.SetPosition(1, baseSegment.EndPoint.transform.position);
        }

        private void UpdateGhostLine(Vector3 perpDir)
        {
            if (ghostLineRenderer == null) return;
            Plane plane = new Plane(Vector3.up, baseSegment.StartPoint.transform.position);
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 hitPoint;
            if (plane.Raycast(ray, out float enter))
            {
                hitPoint = ray.GetPoint(enter);
            }
            else
            {
                hitPoint = baseSegment.StartPoint.transform.position;
            }

            Shape hoveredShape;
            PerformDrawing.RaycastMouse(out Vector3 shapeHitPos, out hoveredShape);
            if (hoveredShape is Point hoverPoint)
            {
                Vector3 anchor = hoverPoint.transform.position;
                // Determine which perpendicular direction (perpDir or -perpDir) is closer to the cursor from the anchor
                float dot = Vector3.Dot(perpDir, hitPoint - anchor);
                Vector3 dir = (dot >= 0) ? perpDir : -perpDir;
                ghostLineRenderer.SetPosition(0, anchor);
                ghostLineRenderer.SetPosition(1, anchor + dir * 3f);
            }
            else
            {
                Vector3 mid = hitPoint;
                ghostLineRenderer.SetPosition(0, mid - perpDir * 1.5f);
                ghostLineRenderer.SetPosition(1, mid + perpDir * 1.5f);
            }
        }

        private void CleanupPreview()
        {
            if (ghostLine != null)
            {
                Object.Destroy(ghostLine);
                ghostLine = null;
                ghostLineRenderer = null;
            }
        }
    }
}