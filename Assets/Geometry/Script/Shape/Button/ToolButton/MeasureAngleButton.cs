using System.Collections;
using UnityEngine;

namespace Manipulator
{
    public class MeasureAngleButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(MeasureAndEditAngle());
        }

        private IEnumerator MeasureAndEditAngle()
        {
            ManipulationManager.Instance.IsDrawing = true;
            UIHint.Show("Tap the first segment");
            yield return ShapePicker.WaitForSegment();
            yield return new WaitUntil(() => !Input.GetMouseButton(0));

            Segment segA = ShapePicker.LastPicked as Segment;
            if (segA == null)
            {
                UIHint.ShowTemp("Invalid segment!", 1.5f);
                ManipulationManager.Instance.IsDrawing = false;
                yield break;
            }

            UIHint.Show("Tap the second segment (must share a point)");
            yield return ShapePicker.WaitForSegment();
            yield return new WaitUntil(() => !Input.GetMouseButton(0));
            UIHint.Hide();

            Segment segB = ShapePicker.LastPicked as Segment;
            if (segB == null || segB == segA)
            {
                UIHint.ShowTemp("Invalid second segment!", 1.5f);
                ManipulationManager.Instance.IsDrawing = false;
                yield break;
            }

            // Find shared point and the two endpoints
            Point shared = null, endA = null, endB = null;
            if (segA.StartPoint == segB.StartPoint) shared = segA.StartPoint;
            else if (segA.StartPoint == segB.EndPoint) shared = segA.StartPoint;
            else if (segA.EndPoint == segB.StartPoint) shared = segA.EndPoint;
            else if (segA.EndPoint == segB.EndPoint) shared = segA.EndPoint;

            if (shared == null)
            {
                UIHint.ShowTemp("Segments do not connect!", 1.5f);
                ManipulationManager.Instance.IsDrawing = false;
                yield break;
            }
            endA = (segA.StartPoint == shared) ? segA.EndPoint : segA.StartPoint;
            endB = (segB.StartPoint == shared) ? segB.EndPoint : segB.StartPoint;

            Vector3 vA = (endA.transform.position - shared.transform.position).normalized;
            Vector3 vB = (endB.transform.position - shared.transform.position).normalized;
            float currentAngle = Vector3.Angle(vA, vB);

            // Show angle info bar
            bool editing = false;
            MeasureInfoBar.Instance.Show($"Angle: {currentAngle:0.###}°", () =>
            {
                if (editing) return;
                editing = true;
                EditDialog.Instance.Show(currentAngle, newAngle =>
                {
                    editing = false;
                    MeasureInfoBar.Instance.Hide();

                    if (Mathf.Approximately(newAngle, currentAngle) || newAngle <= 0 || newAngle >= 180)
                    {
                        ManipulationManager.Instance.IsDrawing = false;
                        PerformDrawing.ResetMode();
                        return;
                    }

                    // Rotate endB to new angle, keep segA fixed
                    float angleDelta = newAngle - currentAngle;
                    // Pick rotation axis: try to keep it on the same plane (Y up for now)
                    Vector3 axis = Vector3.up; // Optionally: Vector3.Cross(vA, vB).normalized;
                    Quaternion rot = Quaternion.AngleAxis(angleDelta, axis);
                    Vector3 newEndB = shared.transform.position + rot * (endB.transform.position - shared.transform.position);

                    UndoRedoNetworkBridge.Instance.DoAndBroadcast(
                        new MultiMoveShapeAction(new System.Collections.Generic.List<(string, Vector3, Vector3)> {
                            (endB.ShapeId, endB.transform.position, newEndB)
                        })
                    );
                    endB.MoveTo(newEndB, silent: false, queue: false);

                    UIHint.ShowTemp($"Angle updated to {newAngle:0.###}°", 1.5f);
                    ManipulationManager.Instance.IsDrawing = false;
                    PerformDrawing.ResetMode();
                });
            });

            // Wait until user finishes (either hide or edit completes)
            while (MeasureInfoBar.Instance.gameObject.activeSelf)
                yield return null;
        }
    }
}
