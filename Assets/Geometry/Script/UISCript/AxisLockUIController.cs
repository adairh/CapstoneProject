namespace Manipulator
{
    using UnityEngine;
    using UnityEngine.UI;

    public class AxisLockUIController : MonoBehaviour
    {
        public Button btnFree;
        public Button btnLockY;
        public Button btnLockXZ;

        void Start()
        {
            btnFree.onClick.AddListener(() => SetAxisLockMode(0));
            btnLockY.onClick.AddListener(() => SetAxisLockMode(1));
            btnLockXZ.onClick.AddListener(() => SetAxisLockMode(2));
            RefreshButtonHighlights();
        }

        public void SetAxisLockMode(int mode)
        {
            ManipulationManager.Instance.CurrentAxisLock = (AxisLockMode)mode;
            RefreshButtonHighlights();
        }

        void RefreshButtonHighlights()
        {
            var current = ManipulationManager.Instance.CurrentAxisLock;
            // Highlight the active button (change color, sprite, etc.)
            btnFree.GetComponent<Image>().color  = (current == AxisLockMode.None)   ? Color.green : Color.white;
            btnLockY.GetComponent<Image>().color = (current == AxisLockMode.LockY)  ? Color.green : Color.white;
            btnLockXZ.GetComponent<Image>().color= (current == AxisLockMode.LockXZ) ? Color.green : Color.white;
        }
    }

}