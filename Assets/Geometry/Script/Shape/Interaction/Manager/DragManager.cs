using UnityEngine;
using UnityEngine.UI;

namespace Manipulator
{
    public class DragManager : MonoBehaviour
    {
        public static DragManager Instance { get; private set; }


        public enum DragState
        {
            XZ,
            Y,
            None
        }

        public DragState currentState;

        private DraggableShape currentDraggingObject = null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

        }


        public bool StartDragging(DraggableShape shape)
        {
            if (currentDraggingObject == null && currentState != DragState.None)
            {
                currentDraggingObject = shape;
                //shape.SetDragging(true);
                return true;
            }

            return false;
        }

        public void StopDragging(DraggableShape shape)
        {
            if (currentDraggingObject == shape)
            {
                //shape.SetDragging(false);
                currentDraggingObject = null;
            }
        }

        public Vector3 GetAllowedAxis()
        {
            switch (currentState)
            {
                case DragState.XZ: return new Vector3(1, 0, 1);
                case DragState.Y: return new Vector3(0, 1, 0);
                default: return Vector3.zero;
            }
        }
    }
}