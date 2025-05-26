using System.Collections;

namespace Manipulator
{
    public class MidpointButton : BaseButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            StartCoroutine(SelectSegmentAndCreateMidpoint());
        }

        private IEnumerator SelectSegmentAndCreateMidpoint()
        {
            // Check: Are there any segments in the scene?
            bool anySegment = false;
            foreach (var shape in ShapeStorage.GetAllShapes())
                if (shape is Segment) { anySegment = true; break; }
            if (!anySegment)
            {
                UIHint.ShowTemp("No segment in scene!", 2f);
                yield break;
            }

            yield return ShapePicker.WaitForSegment("Select a segment to find its midpoint (Esc to cancel)");
            var segment = ShapePicker.LastPicked as Segment;
            if (segment == null) yield break;

            // Get segment's endpoints
            var a = segment.StartPoint;
            var b = segment.EndPoint;

            // Call your midpoint creation tool
            MidpointTool.CreateMidpoint(a, b);

            // Optionally: flash the new point, select it, etc.
        }
    }
}