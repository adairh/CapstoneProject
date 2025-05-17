using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Geometry.Script.UIScript
{
    [RequireComponent(typeof(ScrollRect))]
    public class RuntimeInspector : MonoBehaviour
    {
        [Tooltip("Component chứa các property cần hiển thị.")]
        public MonoBehaviour targetComponent;

        [Header("Prefabs kiểm soát")] public GameObject boolControlPrefab;

        public GameObject intControlPrefab;
        public GameObject floatControlPrefab;
        public GameObject enumControlPrefab;
        public GameObject labelPrefab;

        [Tooltip("RectTransform làm Viewport Content của ScrollRect.")]
        public RectTransform contentParent;

        private void Start()
        {
            if (targetComponent == null || contentParent == null)
            {
                Debug.LogError("RuntimeInspector: Thiếu targetComponent hoặc contentParent!");
                return;
            }

            BuildInspector();
        }

        private void BuildInspector()
        {
            // Xóa sạch UI cũ
            foreach (Transform c in contentParent)
                Destroy(c.gameObject);

            var type = targetComponent.GetType();
            var props = type.GetProperties(
                    BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly // chỉ của class này
                )
                .Where(p => p.CanRead && p.CanWrite);

            foreach (var prop in props)
            {
                var t = prop.PropertyType;
                var name = prop.Name;

                // Hàng ngang 2 cột
                var row = new GameObject("Row_" + name, typeof(RectTransform));
                row.transform.SetParent(contentParent, false);
                var hl = row.AddComponent<HorizontalLayoutGroup>();
                hl.spacing = 8;
                hl.childAlignment = TextAnchor.MiddleLeft;
                hl.childForceExpandWidth = true;
                hl.childForceExpandHeight = false;

                // Cột Label
                var lblGO = Instantiate(labelPrefab, row.transform);
                lblGO.name = "Label_" + name;
                var lbl = lblGO.GetComponent<TextMeshProUGUI>();
                lbl.text = name;
                var le1 = lblGO.GetComponent<LayoutElement>() ?? lblGO.AddComponent<LayoutElement>();
                le1.preferredWidth = 100;

                // Cột Control
                GameObject ctrlGO = null;
                if (t == typeof(bool)) ctrlGO = Instantiate(boolControlPrefab, row.transform);
                else if (t == typeof(int)) ctrlGO = Instantiate(intControlPrefab, row.transform);
                else if (t == typeof(float)) ctrlGO = Instantiate(floatControlPrefab, row.transform);
                else if (t.IsEnum) ctrlGO = Instantiate(enumControlPrefab, row.transform);
                if (ctrlGO == null) continue;
                ctrlGO.name = "Control_" + name;
                var le2 = ctrlGO.GetComponent<LayoutElement>() ?? ctrlGO.AddComponent<LayoutElement>();
                le2.flexibleWidth = 1;

                // Gắn giá trị & sự kiện
                if (t == typeof(bool))
                {
                    var tog = ctrlGO.GetComponent<Toggle>();
                    tog.isOn = (bool)prop.GetValue(targetComponent);
                    tog.onValueChanged.AddListener(v => prop.SetValue(targetComponent, v));
                }
                else if (t == typeof(int))
                {
                    var inp = ctrlGO.GetComponent<TMP_InputField>();
                    inp.contentType = TMP_InputField.ContentType.IntegerNumber;
                    inp.text = prop.GetValue(targetComponent).ToString();
                    inp.onEndEdit.AddListener(s =>
                    {
                        if (int.TryParse(s, out var v))
                            prop.SetValue(targetComponent, v);
                        inp.text = prop.GetValue(targetComponent).ToString();
                    });
                }
                else if (t == typeof(float))
                {
                    var inp = ctrlGO.GetComponent<TMP_InputField>();
                    inp.contentType = TMP_InputField.ContentType.DecimalNumber;
                    inp.text = prop.GetValue(targetComponent).ToString();
                    inp.onEndEdit.AddListener(s =>
                    {
                        if (float.TryParse(s, out var v))
                            prop.SetValue(targetComponent, v);
                        inp.text = prop.GetValue(targetComponent).ToString();
                    });
                }
                else if (t.IsEnum)
                {
                    var dd = ctrlGO.GetComponent<TMP_Dropdown>();
                    var opts = Enum.GetNames(t).ToList();
                    dd.ClearOptions();
                    dd.AddOptions(opts);
                    dd.value = opts.IndexOf(prop.GetValue(targetComponent).ToString());
                    dd.onValueChanged.AddListener(i =>
                    {
                        var val = Enum.Parse(t, opts[i]);
                        prop.SetValue(targetComponent, val);
                    });
                }
            }
        }
    }
}