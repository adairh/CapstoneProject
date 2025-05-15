
using UnityEngine;

namespace Manipulator
{
    public static class SnapHelper
    {
        public static Plane DetermineSnapPlane()
        {
            //Vector3 camForward = Camera.main.transform.forward;
            //Vector3 absDir = new Vector3(Mathf.Abs(camForward.x), Mathf.Abs(camForward.y), Mathf.Abs(camForward.z));

            /*if (absDir.z >= absDir.x && absDir.z >= absDir.y)
                return new Plane(Vector3.forward, 0f);  // Snap to XY (z = 0)
            else if (absDir.x >= absDir.y)
                return new Plane(Vector3.right, 0f);    // Snap to YZ (x = 0)
            else*/
                return new Plane(Vector3.up, 0f);       // Snap to XZ (y = 0)
        }

        public static Vector3 SnapToPlane(Vector3 pos)
        {
            Plane plane = DetermineSnapPlane();
            float dist = plane.GetDistanceToPoint(pos);
            return pos - plane.normal * dist;
        }
    }
}
