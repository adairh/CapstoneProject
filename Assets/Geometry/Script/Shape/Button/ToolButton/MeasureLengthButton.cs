using System.Collections;
using UnityEngine;

namespace Manipulator
{
    public class MeasureLengthButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(MeasureAndEditLength());
        }

        private IEnumerator MeasureAndEditLength()
        {
            ManipulationManager.Instance.IsDrawing = true;
            UIHint.Show("Tap a segment to measure length");
            yield return ShapePicker.WaitForSegment();
            yield return new WaitUntil(() => !Input.GetMouseButton(0));

            Segment seg = ShapePicker.LastPicked as Segment;
            if (seg == null)
            {
                UIHint.ShowTemp("No segment selected!", 1.5f);
                ManipulationManager.Instance.IsDrawing = false;
                yield break;
            }

            float length = Vector3.Distance(seg.StartPoint.transform.position, seg.EndPoint.transform.position);

            // 1. Show bar
            bool editing = false;
            MeasureInfoBar.Instance.Show($"Length: {length:0.###}", () =>
            {
                if (editing) return; // Prevent double popup
                editing = true;
                // 2. Show edit dialog
                EditDialog.Instance.Show(length, newLength =>
                {
                    editing = false;
                    MeasureInfoBar.Instance.Hide();

                    if (Mathf.Approximately(newLength, length) || newLength <= 0)
                    {
                        ManipulationManager.Instance.IsDrawing = false;
                        PerformDrawing.ResetMode();
                        return;
                    }

                    // 3. Move end point (can add choice of end in future)
                    Vector3 dir = (seg.EndPoint.transform.position - seg.StartPoint.transform.position).normalized;
                    Vector3 newEndPos = seg.StartPoint.transform.position + dir * newLength;

                    UndoRedoNetworkBridge.Instance.DoAndBroadcast(
                        new MultiMoveShapeAction(new System.Collections.Generic.List<(string, Vector3, Vector3)> {
                            (seg.EndPoint.ShapeId, seg.EndPoint.transform.position, newEndPos)
                        })
                    );
                    seg.EndPoint.MoveTo(newEndPos, silent: false, queue: false);

                    UIHint.ShowTemp($"Length updated to {newLength:0.###}", 1.5f);
                    ManipulationManager.Instance.IsDrawing = false;
                    PerformDrawing.ResetMode();
                });
            });

            // Wait until user finishes (either Hide or edit completes)
            while (MeasureInfoBar.Instance.gameObject.activeSelf)
                yield return null;
        }
    }
}
