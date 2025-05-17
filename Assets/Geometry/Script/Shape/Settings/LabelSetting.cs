using TMPro;
using UnityEngine;

namespace Manipulator
{
    public class LabelSetting : Setting<string>
    {
        private TMP_InputField inputField;

        public LabelSetting(string initial, Shape shape)
            : base(initial, ISetting.SettingType.NONNUMERIC, typeof(Shape))
        {
            targetShape = shape;
            prefab = UIManager.Instance.GetUIComponent("LabelSettingPrefab");
        }

        public override GameObject GetUI()
        {
            uiInstance = Object.Instantiate(prefab);
            inputField = uiInstance.GetComponentInChildren<TMP_InputField>();
            inputField.text = Value;

            inputField.onEndEdit.AddListener(OnLabelChanged);

            return uiInstance;
        }

        private void OnLabelChanged(string newVal)
        {
            if (newVal == Value) return;

            Value = newVal;
            Apply();
        }

        public override void Apply()
        {
            if (targetShape is Point point)
            {
                //point.UpdateLabelDisplay(Value); // ✅ đã sync qua NetworkVariable bên Point
            }
        }

        public override void Update()
        {
            if (targetShape is Point point)
            {
                Value = point.GetLabel();
                if (inputField != null)
                    inputField.text = Value;
            }
        }

        public override float Height()
        {
            return 50;
        }
    }
}