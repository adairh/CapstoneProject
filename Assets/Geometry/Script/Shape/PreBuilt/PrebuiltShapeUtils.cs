
using UnityEngine;

namespace Manipulator {
    public static class PrebuiltShapeUtils {
        public static Vector3 GetPerpendicular(Vector3 a, Vector3 b) {
            return Vector3.Cross((b - a).normalized, Vector3.up).normalized;
        }
    }
}
