using System.Collections;

namespace Manipulator
{
    public class DivideButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(SelectTwoAndDivide());
        }

        private IEnumerator SelectTwoAndDivide()
        {
            // Check: Are there at least 2 points in the scene?
            int pointCount = 0;
            foreach (var shape in ShapeStorage.GetAllShapes())
                if (shape is Point) pointCount++;
            if (pointCount < 2)
            {
                UIHint.ShowTemp("Not enough points in scene!", 2f);
                yield break;
            }

            yield return ShapePicker.WaitForPoint("Select Point A (Esc to cancel)");
            var a = ShapePicker.LastPicked as Point;
            if (a == null) yield break;

            yield return ShapePicker.WaitForPoint("Select Point B (Esc to cancel)");
            var b = ShapePicker.LastPicked as Point;
            if (b == null) yield break;

            // Optional: Show a preview at the division point (ghost)
            // Optional: Ask for ratio input (currently fixed at 1:2, can pop up a UI)

            DivideSegmentTool.CreateDividedPoint(a, b, 2f); // Default divides at 1:2 ratio

            // Optionally: auto-select or highlight the new point
        }
    }
}