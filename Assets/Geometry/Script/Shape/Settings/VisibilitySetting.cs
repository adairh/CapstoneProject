using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Manipulator
{
    public class VisibilitySetting : Setting<bool>
    {
        public VisibilitySetting(bool initial, Shape shape)
            : base(initial, ISetting.SettingType.NONNUMERIC, typeof(Shape))
        {
            targetShape = shape;
            prefab = UIManager.Instance.GetUIComponent("VisibilitySettingTogglePrefab");
        }

        public override GameObject GetUI()
        {
            uiInstance = Object.Instantiate(prefab);

            var label = uiInstance.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (label != null)
                label.text = "Hiển thị Shape";

            var toggle = uiInstance.GetComponentInChildren<Toggle>();
            toggle.isOn = Value;
            toggle.onValueChanged.AddListener(v =>
            {
                Value = v;
                Apply();
            });

            return uiInstance;
        }

        public override void Apply()
        {
            Debug.Log($"[Apply] Setting = {Value} | Shape = {targetShape?.name}");

            if (targetShape == null) return;

            foreach (var s in targetShape.GetDependentShapesForDelete())
            {
                var rend = s.GetComponentInChildren<Renderer>();
                Debug.Log($"   -> Shape: {s.name}, HasRenderer: {rend != null}");

                if (rend != null)
                {
                    rend.enabled = Value;
                    Debug.Log($"      -> Renderer.enabled = {Value}");
                }
            }

            // 2. Hologram chỉ áp dụng cho shape chính
            var existing = targetShape.GetComponentInChildren<HolographicShapeDisplay>();
            if (!Value && existing == null)
            {
                var go = Object.Instantiate(UIManager.Instance.GetUIComponent("HologramDisplayPrefab"));
                go.transform.position = targetShape.transform.position;
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = Vector3.one * 0.4f;

                var holo = go.GetComponentInChildren<HolographicShapeDisplay>();
                if (holo != null)
                    holo.BindToSetting(this);
            } 

        }

        public override void Update()
        {
            if (targetShape != null)
                Value = targetShape.GetComponentInChildren<Renderer>()?.enabled ?? true;
        }

        public override float Height() => 50;
    }
}
