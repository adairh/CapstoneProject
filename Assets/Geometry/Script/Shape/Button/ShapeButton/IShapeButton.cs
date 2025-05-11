namespace Manipulator
{
    public interface IShapeButton
    {
        ShapeType GetShapeType();

        public enum ShapeType
        {
            None,
            Point,
            Circle,
            Rectangle,
            Triangle,
            Segment,
            StraightLine,
            Line,
            RayShape
        }
    }
}