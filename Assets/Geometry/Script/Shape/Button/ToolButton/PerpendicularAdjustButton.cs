using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class PerpendicularAdjustButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(AdjustPerpendicularRoutine());
        }

        private IEnumerator AdjustPerpendicularRoutine()
        {
            ManipulationManager.Instance.IsDrawing = true;
            UIHint.Show("Select the base segment to align perpendicular to");
            yield return ShapePicker.WaitForSegment();
            Segment baseSegment = ShapePicker.LastPicked as Segment;
            if (baseSegment == null)
            {
                UIHint.ShowTemp("No base segment selected!", 2f);
                ManipulationManager.Instance.IsDrawing = false;
                yield break;
            }

            // --- FIX: Wait for mouse up before next pick ---
            yield return new WaitUntil(() => !Input.GetMouseButton(0));

            UIHint.Show("Select the segment you want to adjust");
            yield return ShapePicker.WaitForSegment();
            Segment targetSegment = ShapePicker.LastPicked as Segment;
            UIHint.Hide();

            if (targetSegment == null || targetSegment == baseSegment)
            {
                UIHint.ShowTemp("Invalid segment selection!", 2f);
                ManipulationManager.Instance.IsDrawing = false;
                yield break;
            }

            // Compute new perpendicular direction (in horizontal plane)
            Vector3 dir = (baseSegment.EndPoint.transform.position - baseSegment.StartPoint.transform.position).normalized;
            Vector3 perpDir = Vector3.Cross(dir, Vector3.up).normalized;
            if (perpDir == Vector3.zero)
                perpDir = Vector3.Cross(dir, Vector3.forward).normalized;

            Vector3 mid = (targetSegment.StartPoint.transform.position + targetSegment.EndPoint.transform.position) / 2f;
            float halfLen = (targetSegment.EndPoint.transform.position - targetSegment.StartPoint.transform.position).magnitude / 2f;
            Vector3 newA = mid - perpDir * halfLen;
            Vector3 newB = mid + perpDir * halfLen;

            // Prepare for undo/redo
            var moves = new List<(string, Vector3, Vector3)>
            {
                (targetSegment.StartPoint.ShapeId, targetSegment.StartPoint.transform.position, newA),
                (targetSegment.EndPoint.ShapeId, targetSegment.EndPoint.transform.position, newB)
            };
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new MultiMoveShapeAction(moves));

            // Actually move the points
            targetSegment.StartPoint.MoveTo(newA, silent: false, queue: false);
            targetSegment.EndPoint.MoveTo(newB, silent: false, queue: false);

            ManipulationManager.Instance.IsDrawing = false;
            PerformDrawing.ResetMode();
        }
    }
}
