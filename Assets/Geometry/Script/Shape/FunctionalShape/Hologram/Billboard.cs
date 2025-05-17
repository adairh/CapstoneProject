using UnityEngine;

namespace Manipulator
{
    public class Billboard : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (Camera.main != null)
                transform.forward = Camera.main.transform.forward;
        }
    }
}