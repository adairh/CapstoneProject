using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Manipulator
{
    public class DynamicInputPanel : MonoBehaviour
    {
        public GameObject fieldPrefab;
        public Transform container;
        private Dictionary<string, TMP_InputField> inputFields = new();

        public void Build(List<FieldDefinition> fields)
        {
            foreach (Transform child in container)
                Destroy(child.gameObject);

            inputFields.Clear();

            foreach (var field in fields)
            {
                var go = Instantiate(fieldPrefab, container);
                var label = go.transform.Find("Label").GetComponent<TMP_Text>();
                var input = go.transform.Find("Input").GetComponent<TMP_InputField>();

                label.text = field.Name;
                inputFields[field.Name] = input;

                if (!field.IsRequired)
                    label.color = Color.gray;
            }
        }

        public Dictionary<string, float> CollectInput()
        {
            var result = new Dictionary<string, float>();
            foreach (var kvp in inputFields)
            {
                if (float.TryParse(kvp.Value.text, out float val))
                    result[kvp.Key] = val;
            }
            return result;
        }

        public void FillCalculatedFields(Dictionary<string, float> values)
        {
            foreach (var kvp in values)
            {
                if (inputFields.ContainsKey(kvp.Key))
                {
                    inputFields[kvp.Key].text = kvp.Value.ToString("F2");
                }
            }
        }
    }
}
