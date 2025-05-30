using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;  // If using the new Input System for Esc key (optional)

namespace Manipulator
{
    public class ParallelButton : BaseButton
    {
        private GameObject ghostLine;
        private LineRenderer ghostLineRenderer;
        private Segment baseSegment; // The selected segment to be parallel to

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
            
            
            StartCoroutine(ParallelLineRoutine());
        }

        private IEnumerator ParallelLineRoutine()
        {
            // Enter drawing mode
            ManipulationManager.Instance.IsDrawing = true;

            // Prompt user to select a base segment
            UIHint.Show("Select a segment to make a parallel line");
            baseSegment = null;
            // Wait for the user to pick a Segment
            yield return ShapePicker.WaitForSegment();
            baseSegment = ShapePicker.LastPicked as Segment;
            UIHint.Hide();

            if (baseSegment == null)
            {
                // No segment selected (shouldn't happen via WaitForSegment unless canceled externally)
                CleanupPreview();
                ManipulationManager.Instance.IsDrawing = false;
                yield break;
            }

            // Segment selected – initialize ghost line preview
            Vector3 baseDir = (baseSegment.EndPoint.transform.position - baseSegment.StartPoint.transform.position)
                .normalized;
            SpawnGhostLine(baseDir);

            // Prompt user to select or click a point for the line to pass through
            UIHint.Show("Select a point through which the parallel line will pass (or click empty space)");
            Point chosenPoint = null;
            Vector3 targetPos = Vector3.zero;
            bool selectionMade = false;

            // Main loop: update preview until selection is made or canceled
            while (!selectionMade)
            {
                // Update ghost line position each frame
                UpdateGhostLine(baseDir);

                // Check for cancel (right-click or Esc)
                if (Input.GetMouseButtonDown(1) || Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    // Cancel the operation
                    UIHint.Hide();
                    CleanupPreview();
                    ManipulationManager.Instance.IsDrawing = false;
                    yield break;
                }

                // Check for left-click selection
                if (Input.GetMouseButtonDown(0))
                {
                    if (PerformDrawing.RaycastMouse(out Vector3 hitPos, out Shape hitShape))
                    {
                        if (hitShape is Point pointShape)
                        {
                            // Existing point selected
                            chosenPoint = pointShape;
                            targetPos = pointShape.transform.position;
                        }
                        else
                        {
                            // Empty space or non-point shape clicked – use the hit position as target
                            chosenPoint = null;
                            targetPos = hitPos;
                        }

                        selectionMade = true;
                        // store targetPos in ManipulationManager for GeometryTool usage
                        ManipulationManager.Instance.TrackingPoint = targetPos;
                    }
                }

                yield return null; // wait for next frame
            }

            // Hide hint and destroy ghost line
            UIHint.Hide();
            CleanupPreview();

            // Create the parallel line through the chosen point/position
            GeometryTool.CreateParallelThrough(
                baseSegment.StartPoint,
                baseSegment.EndPoint,
                chosenPoint,
                3f // using default length
            );

            // The GeometryTool call created the new segment (with a batch action).
            // Now retrieve the new segment's ShapeId from ShapeStorage (it will be the most recently created Segment).
            Segment newSegment = ShapeStorage.GetMostRecentSegment();
            if (newSegment != null)
            {
                // Create and register a Parallel constraint linking baseSegment and newSegment
                var data = new ParallelConstraintData
                {
                    ConstraintId = System.Guid.NewGuid().ToString(),
                    Segment1Id = baseSegment.ShapeId,
                    Segment2Id = newSegment.ShapeId,
                    Type = "Parallel"
                };
                ConstraintFactory.CreateConstraintNetworked(data);
            }

            // Exit drawing mode
            ManipulationManager.Instance.IsDrawing = false;
            
            
            PerformDrawing.ResetMode();
        }

        /// <summary> Spawns a ghost line object with a LineRenderer to preview the new line. </summary>
        private void SpawnGhostLine(Vector3 direction)
        {
            ghostLine = new GameObject("GhostLinePreview");
            ghostLineRenderer = ghostLine.AddComponent<LineRenderer>();
            ghostLineRenderer.positionCount = 2;
            ghostLineRenderer.startWidth = 0.05f;
            ghostLineRenderer.endWidth = 0.05f;
            ghostLineRenderer.material =
                MaterialLibrary.Get(MaterialType.Blue); // Assume a semi-transparent material exists
            ghostLineRenderer.startColor = new Color(1, 1, 1, 0.5f);
            ghostLineRenderer.endColor = new Color(1, 1, 1, 0.5f);
            // Initialize the line in case user hasn't moved yet
            ghostLineRenderer.SetPosition(0, baseSegment.StartPoint.transform.position);
            ghostLineRenderer.SetPosition(1, baseSegment.EndPoint.transform.position);
        }

        /// <summary> Updates the ghost line endpoints based on current mouse position and base direction. </summary>
        private void UpdateGhostLine(Vector3 baseDir)
        {
            if (ghostLineRenderer == null) return;
            // Raycast to plane (horizontal plane through base segment's points)
            Plane plane = new Plane(Vector3.up, baseSegment.StartPoint.transform.position);
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 hitPoint;
            if (plane.Raycast(ray, out float enter))
            {
                hitPoint = ray.GetPoint(enter);
            }
            else
            {
                // Fallback: if plane raycast didn't hit (camera looking parallel to plane), default hitPoint
                hitPoint = baseSegment.StartPoint.transform.position;
            }

            // Determine ghost line endpoints
            // If hovering over an existing point, snap one end to that point
            Shape hoveredShape;
            PerformDrawing.RaycastMouse(out Vector3 shapeHitPos, out hoveredShape);
            if (hoveredShape is Point hoverPoint)
            {
                // Snap ghost line to hoverPoint as one endpoint
                Vector3 anchor = hoverPoint.transform.position;
                // Decide direction: use baseDir (or its opposite) heading toward the cursor relative to anchor
                float dot = Vector3.Dot(baseDir, hitPoint - anchor);
                Vector3 dir = (dot >= 0) ? baseDir : -baseDir;
                ghostLineRenderer.SetPosition(0, anchor);
                ghostLineRenderer.SetPosition(1, anchor + dir * 3f);
            }
            else
            {
                // No specific point hovered – draw line centered at hitPoint
                Vector3 mid = hitPoint;
                ghostLineRenderer.SetPosition(0, mid - baseDir * 1.5f);
                ghostLineRenderer.SetPosition(1, mid + baseDir * 1.5f);
            }
        }

        /// <summary> Destroys the ghost line object if it exists. </summary>
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