using System.Collections;
using UnityEngine;

namespace Manipulator
{
    public static class ShapePicker
    {
        public static Shape LastPicked;

        public static IEnumerator WaitForPoint(string prompt = "Pick a Point")
        {
            LastPicked = null;
            UIHint.Show(prompt);

            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                    if (PerformDrawing.RaycastMouse(out var pos, out var shape))
                        if (shape is Point)
                        {
                            LastPicked = shape;
                            UIHint.Hide();
                            yield break;
                        }

                yield return null;
            }
        }

        public static IEnumerator WaitForSegment(string prompt = "Pick a Segment")
        {
            LastPicked = null;
            UIHint.Show(prompt);

            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                    if (PerformDrawing.RaycastMouse(out var pos, out var shape))
                        if (shape is Segment)
                        {
                            LastPicked = shape;
                            UIHint.Hide();
                            yield break;
                        }

                yield return null;
            }
        }
    }
}