namespace Manipulator
{
    public interface IShapeButton
    {
        public enum ShapeType
        {
            None,
            Point,
            Circle,
            Triangle,
            Segment,
            StraightLine,
            Line,
            RayShape,
            Polygon,

            EquilateralPyramid,
            EquilateralTriangle,
            GenericPyramid,
            IsoscelesTriangle,
            Rectangle,
            RegularTetrahedron,
            Rhombus,
            RightTriangle,
            Square,
            SquarePrism,
            SquarePyramid,
            Tetrahedron
        }

        ShapeType GetShapeType();
    }
}