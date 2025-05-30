using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace Manipulator
{
    public class AIInputController : MonoBehaviour
    {
        [Header("AI Input Elements")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button submitButton;
        [SerializeField] private TMP_Text explanationText;
        [SerializeField] private TMP_Text warningsText;
        [SerializeField] private TMP_Text suggestionsText;

        [Header("Placement Mode")]
        [SerializeField] private Button btnStartPlacement;

        [Header("Open/Close Panel")]
        [SerializeField] private GameObject panelContent; // Usually this.gameObject, or a child panel
        [SerializeField] private Button btnOpenAI;        // Button to open panel (visible when closed)
        [SerializeField] private Button btnCloseAI;       // Button to close panel (visible when open)

        private AIShapeResult pendingShapeResult;
        private bool waitingForPlacement = false;

        private void Start()
        {
            if (submitButton != null)
                submitButton.onClick.AddListener(OnSubmit);

            if (btnStartPlacement != null)
            {
                btnStartPlacement.onClick.AddListener(BeginPlacementMode);
                btnStartPlacement.gameObject.SetActive(false);
            }

            if (btnOpenAI != null)
                btnOpenAI.onClick.AddListener(OpenPanel);

            if (btnCloseAI != null)
                btnCloseAI.onClick.AddListener(ClosePanel);

            // Initial state: panel open, open button hidden
            SetPanelOpen(false);
        }

        private async void OnSubmit()
        {
            UIHint.Show("Đang phân tích và chuẩn bị hình AI...");

            btnStartPlacement?.gameObject.SetActive(false);
            waitingForPlacement = false;
            pendingShapeResult = null;

            string prompt = AIHelper.BuildPrompt(inputField.text);
            string json = await ChatGPTClient.Ask(prompt);

            if (string.IsNullOrWhiteSpace(json))
            {
                UIHint.ShowTemp("Không nhận được phản hồi từ AI.", 2);
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
                UIHint.ShowTemp("Lỗi phân tích kết quả AI.", 2);
                Debug.LogError("[AIInputController] Failed to parse AI response.: " + json);
                return;
            }

            if (result == null)
            {
                UIHint.ShowTemp("Kết quả AI trả về không hợp lệ.", 2);
                Debug.LogError("[AIInputController] Parsed AI result is null.");
                return;
            }

            ShowExplanation(result);

            pendingShapeResult = result;
            waitingForPlacement = true;

            UIHint.Show("Đã sẵn sàng đặt hình. Nhấn nút 'Bắt đầu đặt hình', sau đó click vào không gian để đặt.");
            btnStartPlacement?.gameObject.SetActive(true);
        }

        private void ShowExplanation(AIShapeResult result)
        {
            explanationText.text = result.Explanation ?? "";
            warningsText.text = result.Warnings != null ? string.Join("\n- ", result.Warnings) : "";
            suggestionsText.text = result.Suggestions != null ? string.Join("\n- ", result.Suggestions) : "";
        }

        private void BeginPlacementMode()
        {
            btnStartPlacement?.gameObject.SetActive(false);
            UIHint.Show("Click vào không gian để đặt hình.");
            ManipulationManager.Instance.IsDrawing = true;
            waitingForPlacement = true;
        }

        private void Update()
        {
            if (waitingForPlacement && ManipulationManager.Instance.IsDrawing && pendingShapeResult != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit))
                    {
                        Vector3 placementPoint = hit.point;

                        ShapeExtrasProcessor.BuildFromAIWithOffset(
                            pendingShapeResult.CustomPoints, 
                            pendingShapeResult.ExtraSegments, 
                            placementPoint
                        );

                        UIHint.ShowTemp("Đã đặt hình thành công!", 1.5f);

                        ManipulationManager.Instance.IsDrawing = false;
                        waitingForPlacement = false;
                        pendingShapeResult = null;
                    }
                }
            }
        }

        // Panel open/close logic
        public void OpenPanel()
        {
            SetPanelOpen(true);
        }

        public void ClosePanel()
        {
            SetPanelOpen(false);
        }

        private void SetPanelOpen(bool isOpen)
        {
            if (panelContent != null)
                panelContent.SetActive(isOpen);

            if (btnOpenAI != null)
                btnOpenAI.gameObject.SetActive(!isOpen);

            if (btnCloseAI != null)
                btnCloseAI.gameObject.SetActive(isOpen);

            // Optional: hide placement button if panel is closed
            if (btnStartPlacement != null && !isOpen)
                btnStartPlacement.gameObject.SetActive(false);
        }
    }
}
