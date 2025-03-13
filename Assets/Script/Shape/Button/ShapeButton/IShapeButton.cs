public interface IShapeButton
{
    ShapeType GetShapeType();

    public enum ShapeType
    {
        None,
        Circle,
        Rectangle,
        Triangle
    }
}