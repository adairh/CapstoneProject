using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public static class RelativePointHelper
    {
        /// <summary>
        ///     Dò tam giác trong polygon chứa point, tính tọa độ barycentric và index.
        /// </summary>
        public static bool FindContainingTriangleAndUV(Point targetPoint, Polygon polygon,
            out int indexA, out int indexB, out int indexC,
            out float u, out float v)
        {
            var points = polygon.Points;
            var p = targetPoint.transform.position;

            var triangles = Triangulate(points);

            foreach (var (ia, ib, ic) in triangles)
            {
                var a = points[ia].transform.position;
                var b = points[ib].transform.position;
                var c = points[ic].transform.position;

                var ap = p - a;
                var ab = b - a;
                var ac = c - a;

                var d00 = Vector3.Dot(ab, ab);
                var d01 = Vector3.Dot(ab, ac);
                var d11 = Vector3.Dot(ac, ac);
                var d20 = Vector3.Dot(ap, ab);
                var d21 = Vector3.Dot(ap, ac);
                var denom = d00 * d11 - d01 * d01;

                if (Mathf.Abs(denom) < 1e-6f) continue;

                var _u = (d11 * d20 - d01 * d21) / denom;
                var _v = (d00 * d21 - d01 * d20) / denom;

                if (_u >= 0 && _v >= 0 && _u + _v <= 1f)
                {
                    indexA = ia;
                    indexB = ib;
                    indexC = ic;
                    u = _u;
                    v = _v;
                    return true;
                }
            }

            indexA = indexB = indexC = -1;
            u = v = 0;
            return false;
        }

        /// <summary>
        ///     Tái sử dụng thuật toán chia tam giác như trong Polygon.GenerateMesh()
        /// </summary>
        private static List<(int, int, int)> Triangulate(List<Point> points)
        {
            List<(int, int, int)> triangles = new();
            for (var i = 1; i < points.Count - 1; i++) triangles.Add((0, i, i + 1));
            return triangles;
        }
    }
}