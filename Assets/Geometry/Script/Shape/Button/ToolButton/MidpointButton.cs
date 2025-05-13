
namespace Manipulator
{
    public class MidpointButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(SelectTwoAndMidpoint());
        }

        private System.Collections.IEnumerator SelectTwoAndMidpoint()
        {
            yield return ShapePicker.WaitForPoint("Select First Point");
            var a = ShapePicker.LastPicked as Point;

            yield return ShapePicker.WaitForPoint("Select Second Point");
            var b = ShapePicker.LastPicked as Point;

            if (a != null && b != null)
                MidpointTool.CreateMidpoint(a, b);
        }
    }
}
