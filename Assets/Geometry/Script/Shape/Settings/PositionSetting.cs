using TMPro;
using UnityEngine;

namespace Manipulator
{ 
    public class PositionSetting : Setting<Vector3>
    {
        private TMP_InputField xInput, yInput, zInput;

        public PositionSetting(Shape shape) : base(shape.transform.position, shape,
            UIManager.Instance.GetUIComponent("PositionSettingPrefab")) { }

        public override GameObject CreateUI(Transform parent)
        {
            UIInstance = GameObject.Instantiate(Prefab, parent);

            var inputs = UIInstance.GetComponentsInChildren<TMP_InputField>();
            xInput = inputs[0];
            yInput = inputs[1];
            zInput = inputs[2];

            LoadFromShape();

            xInput.onEndEdit.AddListener(_ => ApplyFromUI());
            yInput.onEndEdit.AddListener(_ => ApplyFromUI());
            zInput.onEndEdit.AddListener(_ => ApplyFromUI());

            return UIInstance;
        }

        public override void LoadFromShape()
        {
            Value = TargetShape.transform.position;
            xInput.text = Value.x.ToString("F2");
            yInput.text = Value.y.ToString("F2");
            zInput.text = Value.z.ToString("F2");
        }

        public override void ApplyToShape()
        {
            TargetShape.MoveTo(Value);
        }

        private void ApplyFromUI()
        {
            if (float.TryParse(xInput.text, out float x) &&
                float.TryParse(yInput.text, out float y) &&
                float.TryParse(zInput.text, out float z))
            {
                Value = new Vector3(x, y, z);
                ApplyToShape();
            }
        }
    }

}