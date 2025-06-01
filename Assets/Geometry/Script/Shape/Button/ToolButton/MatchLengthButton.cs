using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class MatchLengthButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(MatchLengthRoutine());
        }

        private IEnumerator MatchLengthRoutine()
        {
            ManipulationManager.Instance.IsDrawing = true;
            UIHint.Show("Select the base segment to match length with");
            yield return ShapePicker.WaitForSegment();
            Segment baseSegment = ShapePicker.LastPicked as Segment;
            if (baseSegment == null)
            {
                UIHint.ShowTemp("No base segment selected!", 2f);
                ManipulationManager.Instance.IsDrawing = false;
                yield break;
            }

            // Wait for mouse up before next pick
            yield return new WaitUntil(() => !Input.GetMouseButton(0));

            UIHint.Show("Select the segment you want to resize");
            yield return ShapePicker.WaitForSegment();
            Segment targetSegment = ShapePicker.LastPicked as Segment;
            UIHint.Hide();

            if (targetSegment == null || targetSegment == baseSegment)
            {
                UIHint.ShowTemp("Invalid segment selection!", 2f);
                ManipulationManager.Instance.IsDrawing = false;
                yield break;
            }

            // Get the direction of the target segment
            Vector3 dir = (targetSegment.EndPoint.transform.position - targetSegment.StartPoint.transform.position).normalized;
            // Use the length of the base segment
            float newLen = (baseSegment.EndPoint.transform.position - baseSegment.StartPoint.transform.position).magnitude;
            Vector3 mid = (targetSegment.StartPoint.transform.position + targetSegment.EndPoint.transform.position) / 2f;
            float halfLen = newLen / 2f;
            Vector3 newA = mid - dir * halfLen;
            Vector3 newB = mid + dir * halfLen;

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
