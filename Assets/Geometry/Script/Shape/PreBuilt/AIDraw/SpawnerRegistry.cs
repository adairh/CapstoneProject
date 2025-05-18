
using System;
using System.Collections.Generic;
using Manipulator;

namespace Manipulator
{
    public static class SpawnerRegistry
    {
        private static readonly Dictionary<string, IShapeSpawner> spawners = new()
        {
            { "EquilateralPyramid", new Manipulator.EquilateralPyramidSpawner() },
            { "EquilateralTriangle", new Manipulator.EquilateralTriangleSpawner() },
            { "GenericPyramid", new Manipulator.GenericPyramidSpawner() },
            { "IsoscelesTriangle", new Manipulator.IsoscelesTriangleSpawner() },
            { "Rectangle", new Manipulator.RectangleSpawner() },
            { "RegularTetrahedron", new Manipulator.RegularTetrahedronSpawner() },
            { "Rhombus", new Manipulator.RhombusSpawner() },
            { "RightTriangle", new Manipulator.RightTriangleSpawner() },
            { "Square", new Manipulator.SquareSpawner() },
            { "SquarePrism", new Manipulator.SquarePrismSpawner() },
            { "SquarePyramid", new Manipulator.SquarePyramidSpawner() },
            { "Tetrahedron", new Manipulator.TetrahedronSpawner() },
        };

        public static IShapeSpawner Get(string shapeType)
        {
            if (spawners.TryGetValue(shapeType, out var spawner))
                return spawner;
            throw new Exception($"[SpawnerRegistry] ShapeType '{shapeType}' chưa được đăng ký.");
        }
    }
}