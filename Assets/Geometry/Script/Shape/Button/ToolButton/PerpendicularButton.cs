using System.Collections;

namespace Manipulator
{
    public class PerpendicularButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(SelectSegmentAndPoint());
        }

        private IEnumerator SelectSegmentAndPoint()
        {
            yield return ShapePicker.WaitForSegment("Select Segment");
            var seg = ShapePicker.LastPicked as Segment;

            yield return ShapePicker.WaitForPoint("Select Point to pass");
            var p = ShapePicker.LastPicked as Point;

            if (seg != null && p != null)
                GeometryTool.CreatePerpendicularThrough(seg.StartPoint, seg.EndPoint, p, 3f);
        }
    }
}