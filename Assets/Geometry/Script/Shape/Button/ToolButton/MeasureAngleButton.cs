using System.Collections;
using UnityEngine;

namespace Manipulator
{
    public class MeasureAngleButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(SelectThreePointsAndMeasure());
        }

        private IEnumerator SelectThreePointsAndMeasure()
        {
            yield return ShapePicker.WaitForPoint("Select Point A");
            var a = ShapePicker.LastPicked as Point;

            yield return ShapePicker.WaitForPoint("Select Vertex B");
            var b = ShapePicker.LastPicked as Point;

            yield return ShapePicker.WaitForPoint("Select Point C");
            var c = ShapePicker.LastPicked as Point;

            if (a != null && b != null && c != null)
            {
                var go = new GameObject("AngleLabel");
                var label = go.AddComponent<AngleLabel>();
                label.PointA = a;
                label.PointB = b;
                label.PointC = c;
            }
            
            PerformDrawing.ResetMode();
        }
    }
}