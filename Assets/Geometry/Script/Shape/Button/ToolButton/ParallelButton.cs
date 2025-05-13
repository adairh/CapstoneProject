
namespace Manipulator
{
    public class ParallelButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(SelectSegmentAndPoint());
        }

        private System.Collections.IEnumerator SelectSegmentAndPoint()
        {
            yield return ShapePicker.WaitForSegment("Select Segment");
            var seg = ShapePicker.LastPicked as Segment;

            yield return ShapePicker.WaitForPoint("Select Point to pass");
            var p = ShapePicker.LastPicked as Point;

            if (seg != null && p != null)
                GeometryTool.CreateParallelThrough(seg.StartPoint, seg.EndPoint, p, 3f);
        }
    }
}
