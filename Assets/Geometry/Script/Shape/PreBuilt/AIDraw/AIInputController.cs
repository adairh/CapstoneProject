using System.Collections.Generic;
using Manipulator;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Manipulator
{
    public class AIInputController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField aiInputField;
        [SerializeField] private Button askAIButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private TMP_Text warningText;
        [SerializeField] private TMP_Text suggestionText;
        [SerializeField] private DynamicInputPanel inputPanel;

        private IShapeSpawner currentSpawner;
        private Dictionary<string, float> lastSubmittedFields;

        private void Start()
        {
            askAIButton.onClick.AddListener(() => SubmitToAI());
            retryButton.onClick.AddListener(() => ResubmitToAI());
        }

        public async void SubmitToAI()
        {
            string userText = aiInputField.text;
            string prompt = AIHelper.BuildPrompt(userText);
            string json = await ChatGPTClient.Ask(prompt);

            if (string.IsNullOrWhiteSpace(json)) return;

            var result = JsonConvert.DeserializeObject<AIShapeResult>(json);

            Debug.LogWarning(json);

            Debug.LogWarning(result.ShapeType);
            Debug.LogWarning(result.Suggestions);
            Debug.LogWarning(result.Explanation);
            Debug.LogWarning(result.Warnings);
            Debug.LogWarning(result.KnownFields);
            
            
            if (result == null || string.IsNullOrEmpty(result.ShapeType))
            {
                Debug.LogError("[AIInputController] JSON không hợp lệ hoặc thiếu ShapeType");
                return;
            }

            currentSpawner = SpawnerRegistry.Get(result.ShapeType);

            var data = currentSpawner.ComputeShape(result.KnownFields);
            
            ShapeExtrasProcessor.Process(PointMapHelper.From(data), result.CustomPoints, result.ExtraSegments);

            // inputPanel.Build(currentSpawner.GetFieldDefinitions());
            //  
            //
            // inputPanel.FillCalculatedFields(result.KnownFields);
            //
            // lastSubmittedFields = result.KnownFields;

            ShowExplanation(result);
        }

        private async void ResubmitToAI()
        {
            if (lastSubmittedFields == null || lastSubmittedFields.Count == 0)
            {
                Debug.LogWarning("Không có dữ kiện nào để gửi lại AI.");
                return;
            }

            string editedText = aiInputField.text;
            string prompt = AIHelper.BuildPrompt(editedText);
            string json = await ChatGPTClient.Ask(prompt);

            if (string.IsNullOrWhiteSpace(json)) return;

            AIShapeResult result = JsonUtility.FromJson<AIShapeResult>(json);

            if (result != null && !string.IsNullOrEmpty(result.ShapeType))
            {
                currentSpawner = SpawnerRegistry.Get(result.ShapeType);
                inputPanel.Build(currentSpawner.GetFieldDefinitions());
                inputPanel.FillCalculatedFields(result.KnownFields);
                ShowExplanation(result);
            }
        }

        private void ShowExplanation(AIShapeResult result)
        {
            warningText.text = result.Warnings != null && result.Warnings.Length > 0
                ? "\u26A0\ufe0f " + string.Join("\n- ", result.Warnings)
                : "";

            suggestionText.text = result.Suggestions != null && result.Suggestions.Length > 0
                ? "\ud83e\uddd0 " + string.Join("\n- ", result.Suggestions)
                : "";
        }
    }
}
