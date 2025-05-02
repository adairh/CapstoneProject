using TMPro;
using UnityEngine;

namespace Manipulator
{
    public class AngleSetting : Setting<float>
    {
        private readonly AngleConstraint _constraint;
        private readonly GameObject _prefab;
        private GameObject _uiInstance;

        public AngleSetting(AngleConstraint constraint, HologramLabel targetShape) 
            : base(constraint.GetAngle(), ISetting.SettingType.NUMERIC, typeof(HologramLabel))
        {
            _constraint = constraint;
            // Lấy prefab từ UIManager (khai báo sẵn trong scene)
            _prefab = UIManager.Instance.GetUIComponent("AngleSettingPrefab");
            Value = constraint.GetAngle();
            this.targetShape = targetShape;
        }

        public override GameObject GetUI()
        {
            // Instantiate panel
            _uiInstance = Object.Instantiate(_prefab);
            // Tìm TMP_InputField duy nhất trong prefab
            var input = _uiInstance.GetComponentInChildren<TMP_InputField>();
            if (input == null)
            {
                Debug.LogError("AngleSettingPrefab phải có một TMP_InputField");
                return _uiInstance;
            }

            // Hiển thị giá trị hiện tại
            input.text = Value.ToString("F1");

            // Khi edit xong
            input.onEndEdit.AddListener(text =>
            {
                if (float.TryParse(text, out float v))
                {
                    Value = v;
                    Apply();
                }
                // Cập nhật lại text (định dạng)
                input.text = Value.ToString("F1");
            });

            return _uiInstance;
        }

        public override void Apply()
        {
            // Đẩy giá trị mới vào constraint
            _constraint.SetAngle(Value);

            AngleConstraint _angleConstraint = (AngleConstraint)((HologramLabel)targetShape).GetConstraint();
            
            _constraint.RotateOther(_angleConstraint.GetA(), _angleConstraint.GetB(), false);
        }

        public override void Update()
        {
            // Đồng bộ lại từ constraint (nếu thay đổi từ code)
            Value = _constraint.GetAngle();
        }

        public override float Height()
        {
            if (_prefab.TryGetComponent<RectTransform>(out var rt))
                return rt.rect.height;
            return 0f;
        }
    }
}
