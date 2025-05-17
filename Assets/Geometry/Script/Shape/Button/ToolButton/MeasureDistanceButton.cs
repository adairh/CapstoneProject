using System.Collections;
using UnityEngine;

namespace Manipulator
{
    public class MeasureDistanceButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(SelectTwoPointsAndMeasure());
        }

        private IEnumerator SelectTwoPointsAndMeasure()
        {
            yield return ShapePicker.WaitForPoint("Select First Point");
            var a = ShapePicker.LastPicked as Point;

            yield return ShapePicker.WaitForPoint("Select Second Point");
            var b = ShapePicker.LastPicked as Point;

            if (a != null && b != null)
            {
                var go = new GameObject("DistanceLabel");
                var label = go.AddComponent<DistanceLabel>();
                label.PointA = a;
                label.PointB = b;
            }
        }
    }
}