
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace Manipulator
{
    public class AIInputController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button submitButton;
        [SerializeField] private TMP_Text explanationText;
        [SerializeField] private TMP_Text warningsText;
        [SerializeField] private TMP_Text suggestionsText;

        private void Start()
        {
            submitButton.onClick.AddListener(OnSubmit);
        }

        private async void OnSubmit()
        {
            string prompt = AIHelper.BuildPrompt(inputField.text);
            string json = await ChatGPTClient.Ask(prompt);

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("[AIInputController] Empty response from AI.");
                return;
            }

            Debug.Log("[AIInputController] Raw AI response:" + json);

            AIShapeResult result;
            try
            {
                result = JsonConvert.DeserializeObject<AIShapeResult>(json);
            }
            catch
            {
                Debug.LogError("[AIInputController] Failed to parse AI response.");
                return;
            }

            if (result == null)
            {
                Debug.LogError("[AIInputController] Parsed AI result is null.");
                return;
            }

            ShapeExtrasProcessor.BuildFromAI(result.CustomPoints, result.ExtraSegments);
            ShowExplanation(result);
        }

        private void ShowExplanation(AIShapeResult result)
        {
            explanationText.text = result.Explanation ?? "";
            warningsText.text = result.Warnings != null ? string.Join("\n- ", result.Warnings) : "";
            suggestionsText.text = result.Suggestions != null ? string.Join("\n- ", result.Suggestions) : "";
        }
    }
}
