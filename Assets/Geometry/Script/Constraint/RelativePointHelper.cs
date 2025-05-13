
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public static class RelativePointHelper
    {
        /// <summary>
        /// Dò tam giác trong polygon chứa point, tính tọa độ barycentric và index.
        /// </summary>
        public static bool FindContainingTriangleAndUV(Point targetPoint, Polygon polygon,
            out int indexA, out int indexB, out int indexC,
            out float u, out float v)
        {
            var points = polygon.Points;
            Vector3 p = targetPoint.transform.position;

            var triangles = Triangulate(points);

            foreach (var (ia, ib, ic) in triangles)
            {
                Vector3 a = points[ia].transform.position;
                Vector3 b = points[ib].transform.position;
                Vector3 c = points[ic].transform.position;

                Vector3 ap = p - a;
                Vector3 ab = b - a;
                Vector3 ac = c - a;

                float d00 = Vector3.Dot(ab, ab);
                float d01 = Vector3.Dot(ab, ac);
                float d11 = Vector3.Dot(ac, ac);
                float d20 = Vector3.Dot(ap, ab);
                float d21 = Vector3.Dot(ap, ac);
                float denom = d00 * d11 - d01 * d01;

                if (Mathf.Abs(denom) < 1e-6f) continue;

                float _u = (d11 * d20 - d01 * d21) / denom;
                float _v = (d00 * d21 - d01 * d20) / denom;

                if (_u >= 0 && _v >= 0 && (_u + _v) <= 1f)
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
        /// Tái sử dụng thuật toán chia tam giác như trong Polygon.GenerateMesh()
        /// </summary>
        private static List<(int, int, int)> Triangulate(List<Point> points)
        {
            List<(int, int, int)> triangles = new();
            for (int i = 1; i < points.Count - 1; i++)
            {
                triangles.Add((0, i, i + 1));
            }
            return triangles;
        }
    }
}
