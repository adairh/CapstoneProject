using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Manipulator
{
    public class DynamicInputPanel : MonoBehaviour
    {
        public GameObject fieldPrefab;
        public Transform container;
        private readonly Dictionary<string, TMP_InputField> inputFields = new();

        public void Build(List<FieldDefinition> fields)
        {
            if (!IsInScene(container))
            {
                Debug.LogError("DynamicInputPanel: container is not a scene object. Assign a scene instance!");
                return;
            }

            Clear(); // 👈 đảm bảo xóa hợp lệ

            foreach (var field in fields)
            {
                var go = Instantiate(fieldPrefab, container);
                go.name = $"Field_{field.Name}";

                var label = go.transform.Find("Label")?.GetComponent<TMP_Text>();
                var input = go.transform.Find("Input")?.GetComponent<TMP_InputField>();

                if (label == null || input == null)
                {
                    Debug.LogWarning($"Field prefab missing Label or Input for: {field.Name}");
                    continue;
                }

                label.text = field.Name;
                inputFields[field.Name] = input;

                if (!field.IsRequired)
                    label.color = Color.gray;
            }
        }

        public void Clear()
        {
            foreach (Transform child in container)
                if (child.GetComponent<InputField>() || child.name.StartsWith("Input"))
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(child.gameObject);
                    else
#endif
                        Destroy(child.gameObject);
            inputFields.Clear();
        }

        private bool IsInScene(Transform t)
        {
            return t != null && t.gameObject.scene.IsValid();
        }

        public Dictionary<string, float> CollectInput()
        {
            var result = new Dictionary<string, float>();
            foreach (var kvp in inputFields)
                if (float.TryParse(kvp.Value.text, out var val))
                    result[kvp.Key] = val;
            return result;
        }

        public void FillCalculatedFields(Dictionary<string, float> values)
        {
            foreach (var kvp in values)
                if (inputFields.TryGetValue(kvp.Key, out var input))
                    input.text = kvp.Value.ToString("F2");
        }
    }
}