using TMPro;
using UnityEngine;

namespace Manipulator
{
    public class LabelDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        public void Initialize(string label)
        {
            if (text != null)
                text.text = label;
        }

        public void SetLabel(string newText)
        {
            if (text != null)
                text.text = newText;
        }

        public string GetLabel() => text.text;

        private void LateUpdate()
        {
            if (Camera.main != null)
                transform.forward = Camera.main.transform.forward;
        }
    }
}