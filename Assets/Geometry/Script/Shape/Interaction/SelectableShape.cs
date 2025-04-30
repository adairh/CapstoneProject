using UnityEngine;

namespace Manipulator
{
    //[RequireComponent(typeof(Collider))]
    public class SelectableShape : MonoBehaviour
    {
        private Shape _shape;

        /// <summary>
        /// Phải gọi ngay sau khi Instantiate shape: shapeGO.GetComponent<SelectableShape>().SetShape(myShape);
        /// </summary>
        public void SetShape(Shape shape)
        {
            _shape = shape;
        }

        private void OnMouseDown()
        {
            // Chỉ xử lý khi nhấn Ctrl + click
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                ManipulationManager.Instance.ToggleSelection(_shape);
            }
        }
    }
}