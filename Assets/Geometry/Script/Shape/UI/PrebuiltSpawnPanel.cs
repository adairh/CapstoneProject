using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Manipulator
{
    public class PrebuiltSpawnPanel : MonoBehaviour
    {
        public static PrebuiltSpawnPanel Instance;

        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform inputContainer;
        [SerializeField] private GameObject inputPrefab;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private readonly List<TMP_InputField> inputs = new();
        private Action<float[]> onConfirm;

        private void Awake()
        {
            Instance = this;
            root.SetActive(false);
            confirmButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Close);
        }

        public static void Show(string title, string[] fields, Action<float[]> callback)
        {
            Instance.root.SetActive(true);
            Instance.titleText.text = title;
            Instance.onConfirm = callback;

            foreach (Transform child in Instance.inputContainer)
                Destroy(child.gameObject);
            Instance.inputs.Clear();

            foreach (var field in fields)
            {
                var go = Instantiate(Instance.inputPrefab, Instance.inputContainer);
                go.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = field;
                var input = go.transform.GetChild(1).GetComponent<TMP_InputField>();
                input.text = "1";
                Instance.inputs.Add(input);
            }
        }

        private void Confirm()
        {
            var values = new List<float>();
            foreach (var input in inputs)
                if (float.TryParse(input.text, out var val))
                {
                    values.Add(val);
                }
                else
                {
                    Debug.LogWarning("Giá trị nhập không hợp lệ!");
                    return;
                }

            root.SetActive(false);
            onConfirm?.Invoke(values.ToArray());
        }

        private void Close()
        {
            root.SetActive(false);
        }
    }
}