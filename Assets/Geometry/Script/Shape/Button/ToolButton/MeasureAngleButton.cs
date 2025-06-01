using System.Collections;
using UnityEngine;

namespace Manipulator
{
    public class MeasureAngleButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(MeasureRoutine());
        }

        private IEnumerator MeasureRoutine()
        {
            PerformDrawing.ResetMode();
            UIHint.Show("Tap point A");
            yield return ShapePicker.WaitForPoint();
            var a = ShapePicker.LastPicked as Point;
            if (a == null) yield break;

            UIHint.Show("Tap vertex point B");
            yield return ShapePicker.WaitForPoint();
            var b = ShapePicker.LastPicked as Point;
            if (b == null) yield break;

            UIHint.Show("Tap point C");
            yield return ShapePicker.WaitForPoint();
            UIHint.Hide();
            var c = ShapePicker.LastPicked as Point;
            if (c == null || b == a || b == c || a == c) yield break;

            ShowAngle(a, b, c);
        }

        private void ShowAngle(Point a, Point b, Point c)
        {
            float angle = Vector3.Angle(a.transform.position - b.transform.position, c.transform.position - b.transform.position);

            MeasureInfoBar.Instance.Show(
                $"Angle at B: {angle:F1}°",
                () => EditDialog.Instance.Show(angle, val => EditAngle(a, b, c, val))
            );
        }

        // Move C so angle at B is 'newAngle' (keep A, B fixed, rotate C about B)
        private void EditAngle(Point a, Point b, Point c, float newAngle)
        {
            // Fix lengths, only move C around B so angle ABC = newAngle
            Vector3 ba = (a.transform.position - b.transform.position).normalized;
            float bcDist = (c.transform.position - b.transform.position).magnitude;

            // Calculate new direction for BC such that angle ABC = newAngle
            Quaternion rot = Quaternion.AngleAxis(newAngle, Vector3.up); // Or choose plane normal
            Vector3 newBC = rot * ba * bcDist;
            Vector3 newC = b.transform.position + newBC;

            // Undo/redo support if needed:
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(
               new MoveShapeAction(c.ShapeId, c.transform.position, newC));

            c.MoveTo(newC, silent: false, queue: false);
            ShowAngle(a, b, c); // Update info bar with new value
        }
    }
}
