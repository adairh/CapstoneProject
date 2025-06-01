using System.Collections;
using UnityEngine;

namespace Manipulator
{
    public class MeasureDistanceButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(MeasureRoutine());
        }

        private IEnumerator MeasureRoutine()
        {
            PerformDrawing.ResetMode();
            UIHint.Show("Tap first point");
            yield return ShapePicker.WaitForPoint();
            var a = ShapePicker.LastPicked as Point;
            if (a == null) yield break;

            UIHint.Show("Tap second point");
            yield return ShapePicker.WaitForPoint();
            var b = ShapePicker.LastPicked as Point;
            UIHint.Hide();
            if (a == null || b == null || a == b) yield break;

            ShowDistance(a, b);
        }

        private void ShowDistance(Point a, Point b)
        {
            float dist = Vector3.Distance(a.transform.position, b.transform.position);

            MeasureInfoBar.Instance.Show(
                $"Distance: {dist:F2} units",
                () => EditDialog.Instance.Show(dist, val => EditDistance(a, b, val))
            );
        }

        // Edits distance by moving b along direction AB to match 'newDist', keeping 'a' fixed.
        private void EditDistance(Point a, Point b, float newDist)
        {
            Vector3 dir = (b.transform.position - a.transform.position).normalized;
            Vector3 newB = a.transform.position + dir * newDist;

            // Undo/redo support if needed:
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(
               new MoveShapeAction(b.ShapeId, b.transform.position, newB));

            b.MoveTo(newB, silent: false, queue: false);
            ShowDistance(a, b); // Update info bar with new value
        }
    }
}