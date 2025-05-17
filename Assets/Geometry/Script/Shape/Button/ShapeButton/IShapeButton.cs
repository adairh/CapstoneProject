namespace Manipulator
{
    public interface IShapeButton
    {
        public enum ShapeType
        {
            None,
            Point,
            Circle,
            Segment,
            StraightLine,
            Line,
            RayShape,
            Polygon,

            Triangle,
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
            Tetrahedron,
            
            TriangleSpawner,
            EquilateralPyramidSpawner,
            EquilateralTriangleSpawner,
            GenericPyramidSpawner,
            IsoscelesTriangleSpawner,
            RectangleSpawner,
            RegularTetrahedronSpawner,
            RhombusSpawner,
            RightTriangleSpawner,
            SquareSpawner,
            SquarePrismSpawner,
            SquarePyramidSpawner,
            TetrahedronSpawner
        }

        ShapeType GetShapeType();
    }
}