
namespace Manipulator
{
    public class DivideButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(SelectTwoAndDivide());
        }

        private System.Collections.IEnumerator SelectTwoAndDivide()
        {
            yield return ShapePicker.WaitForPoint("Select Point A");
            var a = ShapePicker.LastPicked as Point;

            yield return ShapePicker.WaitForPoint("Select Point B");
            var b = ShapePicker.LastPicked as Point;

            if (a != null && b != null)
                DivideSegmentTool.CreateDividedPoint(a, b, 2f); // mặc định chia 1:2
        }
    }
}
